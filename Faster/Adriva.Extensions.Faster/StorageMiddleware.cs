using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Formats.Asn1;
using System.IO;
using System.IO.Pipelines;
using System.Net.Mime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Adriva.Common.Core;
using FASTER.core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Session;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace Adriva.Extensions.Faster
{
    public sealed class StorageMiddleware : IMiddleware
    {
        private readonly IFasterStorageClient FasterStorageClient;
        private readonly FasterOptions Options;
        private readonly ObjectPool<StringBuilder> StringBuilderPool;

        private readonly string LockOperationPath;
        private readonly string ReleaseLockOperationPath;

        public StorageMiddleware(ObjectPoolProvider objectPoolProvider, IOptions<FasterOptions> optionsAccessor, IFasterStorageClient fasterStorageClient)
        {
            this.FasterStorageClient = fasterStorageClient;
            this.Options = optionsAccessor.Value;
            this.StringBuilderPool = objectPoolProvider.CreateStringBuilderPool();

            this.LockOperationPath = this.Options.PathBase.Add("/lock");
            this.ReleaseLockOperationPath = this.Options.PathBase.Add("/lock/release");
        }

        private Task<StorageDataEntry> GetAsync(string key) => this.FasterStorageClient.GetAsync(key, true);

        private ValueTask<string> UpsertAsync(string key, object data, string etag, TimeSpan? timeToLive = null) => this.FasterStorageClient.UpsertAsync(key, data, etag, timeToLive);

        private ValueTask<string> UpsertAsync(ref CacheUpsertRequest upsertRequest)
        {
            string key = upsertRequest.Key;
            object data = upsertRequest.Value;
            string etag = upsertRequest.ETag;
            int? timeToLive = upsertRequest.TimeToLive;
            TimeSpan? normalizedTimeToLive = null;

            if (timeToLive.HasValue)
            {
                timeToLive = Math.Max(10, timeToLive.Value);
                normalizedTimeToLive = TimeSpan.FromSeconds(timeToLive.Value);
            }

            return this.UpsertAsync(key, data, etag, normalizedTimeToLive);
        }

        private ValueTask<bool> DeleteAsync(string key) => this.FasterStorageClient.DeleteAsync(key);

        private Task<FasterLockToken> TryAcquireLockAsync(string key, TimeSpan timeToLive) => this.FasterStorageClient.TryAcquireLockAsync(key, timeToLive);

        private ValueTask<bool> TryReleaseLockAsync(FasterLockToken fasterLockToken) => this.FasterStorageClient.ReleaseLockAsync(fasterLockToken);

        private async ValueTask<T> ReadRequestBodyAsync<T>(HttpRequest request, CancellationToken cancellationToken)
        {
            StringBuilder buffer = this.StringBuilderPool.Get();

            try
            {
                do
                {
                    ReadResult readResult = await request.BodyReader.ReadAsync(cancellationToken);
                    buffer.Append(Encoding.UTF8.GetString(readResult.Buffer));
                    request.BodyReader.AdvanceTo(readResult.Buffer.Start, readResult.Buffer.End);

                    if (readResult.IsCompleted || readResult.IsCanceled)
                    {
                        await request.BodyReader.CompleteAsync();
                        break;
                    }
                } while (true);
                return Utilities.SafeDeserialize<T>(buffer.ToString());
            }
            finally
            {
                this.StringBuilderPool.Return(buffer);
            }
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            StorageDataEntry output;

            if (HttpMethods.IsGet(context.Request.Method) || HttpMethods.IsHead(context.Request.Method))
            {
                output = await this.GetAsync(context.Request.Query["key"]);
                if (StorageDataEntry.Empty == output)
                {
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                }
                else
                {
                    context.Response.Headers.ETag = output.ETag;

                    if (HttpMethods.IsHead(context.Request.Method))
                    {
                        context.Response.StatusCode = StatusCodes.Status204NoContent;
                    }
                    else
                    {
                        context.Response.StatusCode = StatusCodes.Status200OK;
                        context.Response.ContentType = MediaTypeNames.Application.Json;
                        await context.Response.WriteAsync(Utilities.SafeSerialize(output.Data), Encoding.UTF8);
                    }
                }
            }
            else if (HttpMethods.IsPut(context.Request.Method))
            {
                try
                {
                    using (CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(10_000))
                    {
                        var upsertRequest = await this.ReadRequestBodyAsync<CacheUpsertRequest>(context.Request, cancellationTokenSource.Token);

                        if (0 < context.Request.Headers.IfMatch.Count)
                        {
                            upsertRequest.ETag = context.Request.Headers.IfMatch[0];
                        }

                        context.Response.StatusCode = StatusCodes.Status204NoContent;
                        context.Response.Headers.ETag = await this.UpsertAsync(ref upsertRequest);
                    }
                }
                catch (FasterConflictException)
                {
                    context.Response.StatusCode = StatusCodes.Status409Conflict;
                }
                catch
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                }
            }
            else if (HttpMethods.IsDelete(context.Request.Method))
            {
                if (await this.DeleteAsync(context.Request.Query["key"]))
                {
                    context.Response.StatusCode = StatusCodes.Status204NoContent;
                }
                else
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                }
            }
            else if (HttpMethods.IsPost(context.Request.Method))
            {
                if (context.Request.Path == this.LockOperationPath)
                {
                    using (CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(10_000))
                    {
                        var cacheLockRequest = await this.ReadRequestBodyAsync<CacheLockRequest>(context.Request, cancellationTokenSource.Token);
                        var lockResult = await this.FasterStorageClient.TryAcquireLockAsync(cacheLockRequest.Key, TimeSpan.FromSeconds(cacheLockRequest.TimeToLive ?? 0));

                        if (!lockResult.IsSuccess)
                        {
                            context.Response.StatusCode = StatusCodes.Status409Conflict;
                            return;
                        }
                        else
                        {
                            await context.Response.WriteAsJsonAsync(lockResult);
                        }
                    }
                }
                else if (context.Request.Path == this.ReleaseLockOperationPath)
                {
                    using (CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(10_000))
                    {
                        var lockToken = await this.ReadRequestBodyAsync<FasterLockToken>(context.Request, cancellationTokenSource.Token);

                        if (await this.FasterStorageClient.ReleaseLockAsync(lockToken))
                        {
                            context.Response.StatusCode = StatusCodes.Status204NoContent;
                        }
                        else
                        {
                            context.Response.StatusCode = StatusCodes.Status409Conflict;
                        }
                    }
                }
            }
        }
    }
}