using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Adriva.Common.Core;
using Adriva.Extensions.Caching.Abstractions;
using Microsoft.Extensions.Caching.Distributed;

namespace Adriva.Extensions.Caching.Distributed
{
    public class DistributedCache : ICache
    {
        private readonly IDistributedCache Cache;

        public DistributedCache(IDistributedCache cache)
        {
            this.Cache = cache;
        }

        private async Task SetAsync(string key, DistributedCacheEntry entry)
        {
            if (null == entry) throw new ArgumentNullException(nameof(entry));

            string json = Utilities.SafeSerialize(entry);
            DistributedCacheEntryOptions options = new DistributedCacheEntryOptions()
            {
                AbsoluteExpiration = entry.AbsoluteExpiration,
                AbsoluteExpirationRelativeToNow = entry.AbsoluteExpirationRelativeToNow,
                SlidingExpiration = entry.SlidingExpiration
            };
            await this.Cache.SetStringAsync(key, json, options);
        }

        private async Task<DistributedCacheEntry> GetAsync(string key)
        {
            string json = await this.Cache.GetStringAsync(key);
            return Utilities.SafeDeserialize<DistributedCacheEntry>(json);
        }

        public async Task<T> GetOrCreateAsync<T>(string key, Func<ICacheItem, Task<T>> factory, EvictionCallback evictionCallback = null, params string[] dependencyMonikers)
        {
            if (null != evictionCallback) throw new NotSupportedException("EvictionCallback is not supported for distributed cache.");

            bool shouldCreateNew = false;

            Queue<string> missingDependencyMonikers = new Queue<string>();
            DistributedCacheEntry entry = await this.GetAsync(key);
            shouldCreateNew = null == entry;

            foreach (var dependencyMoniker in dependencyMonikers)
            {
                var dependencyEntry = await this.GetAsync(dependencyMoniker);
                if (null == dependencyEntry)
                {
                    shouldCreateNew = true;
                    missingDependencyMonikers.Enqueue(dependencyMoniker);
                }
            }

            if (!shouldCreateNew)
            {
                return Utilities.SafeDeserialize<T>(entry.Data);
            }

            long timestamp = DateTime.Now.Ticks;
            DistributedCacheEntry newEntry = new DistributedCacheEntry();
            T data = await factory.Invoke(newEntry);
            newEntry.Data = Utilities.SafeSerialize(data);
            newEntry.Timestamp = timestamp;

            await this.SetAsync(key, newEntry);

            while (missingDependencyMonikers.TryDequeue(out string missingDependencyMoniker))
            {
                await this.SetAsync(missingDependencyMoniker, new DistributedCacheEntry()
                {
                    Timestamp = timestamp,
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(7)
                });
            }

            return Utilities.SafeDeserialize<T>(newEntry.Data);
        }

        public Task<T> GetOrCreateAsync<T>(string key, Func<ICacheItem, Task<T>> factory, params string[] dependencyMonikers)
        {
            return this.GetOrCreateAsync(key, factory, null, dependencyMonikers);
        }

        public virtual void NotifyChanged(string key, string dependencyMoniker)
        {
            this.Cache.Remove(dependencyMoniker);
        }

        public virtual async Task NotifyChangedAsync(string key, string dependencyMoniker)
        {
            await this.Cache.RemoveAsync(dependencyMoniker);
        }

        public async ValueTask RemoveAsync(string key)
        {
            await this.Cache.RemoveAsync(key);
        }
    }
}
