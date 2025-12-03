using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FASTER.core;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

using AdrivaClientSession = FASTER.core.ClientSession<string, Adriva.Extensions.Faster.StorageDataEntry, Adriva.Extensions.Faster.StorageDataEntry, Adriva.Extensions.Faster.StorageDataEntry, FASTER.core.Empty, Adriva.Extensions.Faster.CallbackFunctions>;

namespace Adriva.Extensions.Faster
{
    public sealed class FasterStorageService : IFasterStorageClient, IHostedService, IAsyncDisposable
    {
        private readonly FasterOptions Options;
        private readonly CallbackFunctions CallbackFunctions;

        private FasterKV<string, StorageDataEntry> MicrosoftFaster;

        private static void NormalizeKey(string key, bool isUserKey)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException($"Key cannot be null, empty or whitespace.");
            }

            if (isUserKey && '$' == key[0])
            {
                throw new ArgumentException($"Key cannot start with a '$' character.");
            }
        }

        private static TimeSpan NormalizeTimeSpan(TimeSpan? timeSpan, double minimumSeconds, double maximumSeconds)
        {
            if (!timeSpan.HasValue)
            {
                return TimeSpan.FromDays(360);
            }
            else
            {
                if (0 < minimumSeconds && timeSpan.Value.TotalSeconds < minimumSeconds)
                {
                    timeSpan = TimeSpan.FromSeconds(minimumSeconds);
                }
                else if (0 < maximumSeconds && timeSpan.Value.TotalSeconds > maximumSeconds)
                {
                    timeSpan = TimeSpan.FromSeconds(maximumSeconds);
                }
            }

            return timeSpan.Value;
        }

        public FasterStorageService(IOptions<FasterOptions> optionsAccessor)
        {
            this.Options = optionsAccessor.Value;
            this.Options.Normalize();

            this.CallbackFunctions = new CallbackFunctions();
        }

        public void Initialize()
        {
            FasterKVSettings<string, StorageDataEntry> settings = new FasterKVSettings<string, StorageDataEntry>()
            {
                ConcurrencyControlMode = this.Options.UseLocks ? ConcurrencyControlMode.RecordIsolation : ConcurrencyControlMode.None,
                MemorySize = this.Options.MemoryLimit,
                ReadCacheMemorySize = this.Options.MemoryCacheLimit,
                EqualityComparer = null,
                PreallocateLog = true,
                LogDevice = new ManagedLocalStorageDevice(Path.Combine("data/faster/log")),
                ObjectLogDevice = new ManagedLocalStorageDevice(Path.Combine("data/faster/obj")),
                TryRecoverLatest = true
            };

            this.MicrosoftFaster = new FasterKV<string, StorageDataEntry>(settings);
        }

        private ValueTask<TOutput> RunInSessionAsync<TOutput>(Func<AdrivaClientSession, ValueTask<TOutput>> sessionAction, string sessionName = null)
        {
            var session = this.MicrosoftFaster
                                .For(this.CallbackFunctions)
                                .NewSession<CallbackFunctions>(sessionName);

            return sessionAction(session);
        }

        public async Task<StorageDataEntry> GetAsync(string key, bool isUserKey)
        {
            FasterStorageService.NormalizeKey(key, isUserKey);

            return await this.RunInSessionAsync(async session =>
            {
                var readResult = await session.ReadAsync(key);

                if (!readResult.Status.IsCompleted)
                {
                    readResult.Complete();
                }

                if (readResult.Status.Expired || readResult.Status.NotFound)
                {
                    return StorageDataEntry.Empty;
                }

                return readResult.Output;
            });
        }

        public async ValueTask<bool> DeleteAsync(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return false;
            }

            var result = await this.RunInSessionAsync(async session =>
            {
                var r = await session.DeleteAsync(key);
                r.Complete();
                return r;
            });


            return result.Status.IsCompletedSuccessfully && !result.Status.IsCanceled && !result.Status.IsFaulted;
        }

        public async ValueTask<string> UpsertAsync(string key, object data, string etag, TimeSpan? expireAfter)
        {
            FasterStorageService.NormalizeKey(key, true);
            expireAfter = FasterStorageService.NormalizeTimeSpan(expireAfter, 5, -1);

            var upsertResult = await this.RunInSessionAsync(async session =>
            {
                StorageDataEntry entry;

                entry = new StorageDataEntry(data, etag, expireAfter.Value);

                var result = await session.UpsertAsync(key, entry);

                if (!result.Status.IsCompleted)
                {
                    result.Complete();
                }

                return result;
            });

            return upsertResult.Output.ETag;
        }

        public async Task<FasterLockToken> TryAcquireLockAsync(string key, TimeSpan autoReleaseTimespan)
        {
            FasterStorageService.NormalizeKey(key, true);
            string randomKey = Guid.NewGuid().ToString("N");

            autoReleaseTimespan = FasterStorageService.NormalizeTimeSpan(autoReleaseTimespan, 5, 24 * 60 * 60);

            return await this.RunInSessionAsync(async session =>
            {
                var result = await session.RMWAsync($"$Lock_{key}", new StorageDataEntry(randomKey, autoReleaseTimespan));

                if (!result.Status.IsCompleted)
                {
                    result.Complete();
                }

                if (result.Status.IsCompleted && !result.Status.IsCanceled && !result.Status.IsFaulted)
                {
                    return new FasterLockToken(true, randomKey, key);
                }
                else
                {
                    return new FasterLockToken(false, null, key);
                }
            }, randomKey);
        }

        public async ValueTask<bool> ReleaseLockAsync(FasterLockToken lockToken)
        {
            return await this.RunInSessionAsync(async session =>
            {
                string lockKey = $"$Lock_{lockToken.Key}";
                var storageDataEntry = new StorageDataEntry(lockToken.Value, TimeSpan.FromMinutes(-1));
                var result = await session.RMWAsync(lockKey, storageDataEntry);

                if (!result.Status.IsCompleted)
                {
                    result.Complete();
                }

                return result.Status.IsCompletedSuccessfully && !result.Status.IsCanceled && !result.Status.IsFaulted && result.Status.Expired;
            });
        }

        public async ValueTask PersistAsync()
        {
            var r1 = await this.MicrosoftFaster.TakeIndexCheckpointAsync();
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            this.Initialize();
        }

        public Task StopAsync(CancellationToken cancellationToken) => this.DisposeAsync().AsTask();

        public async ValueTask DisposeAsync()
        {
            this.MicrosoftFaster?.Dispose();
            await Task.CompletedTask;
        }
    }
}