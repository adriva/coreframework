using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Linq;
using System.Threading.Tasks;
using Adriva.Common.Core;
using Adriva.Storage.Abstractions;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.Options;

namespace Adriva.Storage.Azure
{
    public sealed class AzureBlobClient : IBlobClient
    {
        private readonly IOptionsMonitor<AzureBlobConfiguration> ConfigurationAccessor;

        private AzureBlobConfiguration Configuration;
        private CloudBlobContainer Container;

        public AzureBlobClient(IOptionsMonitor<AzureBlobConfiguration> configurationAccessor)
        {
            this.ConfigurationAccessor = configurationAccessor;
        }

        public async ValueTask InitializeAsync(string clientName)
        {
            this.Configuration = this.ConfigurationAccessor.Get(clientName);

            if (!CloudStorageAccount.TryParse(this.Configuration.ConnectionString, out CloudStorageAccount account))
            {
                throw new InvalidDataException($"Azure blob connection string for blob client '{clientName}' could not be parsed.");
            }
            var blobClient = account.CreateCloudBlobClient();
            this.Container = blobClient.GetContainerReference(this.Configuration.ContainerName);
            await this.Container.CreateIfNotExistsAsync();
        }

        public async ValueTask<bool> ExistsAsync(string name)
        {
            var blob = this.Container.GetBlobReference(name);
            return await blob.ExistsAsync();
        }

        public async Task<Stream> OpenReadStreamAsync(string name)
        {
            var blob = this.Container.GetBlobReference(name);
            return await blob.OpenReadAsync();
        }

        public async Task<ReadOnlyMemory<byte>> ReadAllBytesAsync(string name)
        {
            var blob = this.Container.GetBlobReference(name);
            await blob.FetchAttributesAsync();
            if (1 > blob.Properties.Length) return new ReadOnlyMemory<byte>();

            byte[] buffer = new byte[blob.Properties.Length];
            await blob.DownloadToByteArrayAsync(buffer, 0);
            return new ReadOnlyMemory<byte>(buffer);
        }

        public async Task<string> UpsertAsync(string name, Stream stream, int cacheDuration = 0)
        {
            var blob = this.Container.GetBlockBlobReference(name);
            var mimeType = MimeTypes.GetMimeType(name);
            blob.Properties.ContentType = mimeType;

            if (0 < cacheDuration)
            {
                blob.Properties.CacheControl = $"public, max-age={cacheDuration}";
            }

            await blob.UploadFromStreamAsync(stream);
            return blob.Uri.ToString();
        }

        public async Task<SegmentedResult<string>> ListAllAsync(string continuationToken, string prefix = null, int count = 500)
        {
            count = Math.Min(1000, Math.Max(1, count));
            var blobContinuationToken = AzureStorageUtilities.DeserializeBlobContinuationToken(continuationToken);
            var result = await this.Container.ListBlobsSegmentedAsync(prefix, true, BlobListingDetails.None, count, blobContinuationToken, null, null);
            string nextPageToken = result.ContinuationToken.Serialize();
            return new SegmentedResult<string>(result.Results.OfType<CloudBlob>().Select(b => b.Name), nextPageToken, null != result.ContinuationToken);
        }

        public async Task<string> UpsertAsync(string name, ReadOnlyMemory<byte> data, int cacheDuration = 0)
        {
            using (var stream = new MemoryStream())
            {
                PipeWriter writer = PipeWriter.Create(stream);
                var result = await writer.WriteAsync(data);
                stream.Seek(0, SeekOrigin.Begin);
                return await this.UpsertAsync(name, stream, cacheDuration);
            }
        }

        public async ValueTask DeleteAsync(string name)
        {
            var blob = this.Container.GetBlobReference(name);
            await blob.DeleteIfExistsAsync();
        }

        public ValueTask DisposeAsync()
        {
            this.Container = null;
            return new ValueTask();
        }
    }
}
