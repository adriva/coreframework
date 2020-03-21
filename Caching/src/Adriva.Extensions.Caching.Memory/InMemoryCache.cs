using System;
using System.Threading;
using System.Threading.Tasks;
using Adriva.Extensions.Caching.Abstractions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Adriva.Extensions.Caching.Memory
{
    /// <summary>
    /// Provides an in memory implementation of cache services.
    /// </summary>
    public class InMemoryCache : ICache, IDisposable
    {
        private readonly ILogger Logger;
        private readonly IMemoryCache MemoryCache;
        private readonly ReaderWriterLockSlim Lock = new ReaderWriterLockSlim();

        public InMemoryCache(ILogger<InMemoryCache> logger)
        {
            this.Logger = logger;
            this.MemoryCache = new MemoryCache(new MemoryCacheOptions());
        }

        private void ChangeTokenEvicted(object key, object value, EvictionReason reason, object state)
        {
            if (value is DependencyChangeToken dependencyChangeToken)
            {
                dependencyChangeToken.NotifyChanged();
            }
        }

        public async Task<T> GetOrCreateAsync<T>(string key, Func<ICacheItem, Task<T>> factory, EvictionCallback evictionCallback = null, params string[] dependencyMonikers)
        {
            this.Lock.EnterReadLock();

            try
            {
                this.Logger.LogTrace($"Returning item '{key}' from in-memory cache.");
                return await this.MemoryCache.GetOrCreateAsync<T>(key, async (entry) =>
                {
                    this.Logger.LogTrace($"Item '{key}' not found in in-memory cache. Creating a new one.");
                    if (null != dependencyMonikers)
                    {
                        foreach (string dependencyMoniker in dependencyMonikers)
                        {
                            var changeToken = this.MemoryCache.GetOrCreate(dependencyMoniker, (dependencyEntry) =>
                            {
                                dependencyEntry.RegisterPostEvictionCallback(this.ChangeTokenEvicted);
                                return new DependencyChangeToken();
                            });
                            entry.AddExpirationToken(changeToken);
                            this.Logger.LogTrace($"Change token added to item '{key}'.");
                        }
                    }

                    MemoryCacheItem memoryCacheItem = new MemoryCacheItem(entry);
                    T data = await factory.Invoke(memoryCacheItem);
                    entry.AbsoluteExpiration = memoryCacheItem.AbsoluteExpiration;
                    entry.AbsoluteExpirationRelativeToNow = memoryCacheItem.AbsoluteExpirationRelativeToNow;
                    entry.SlidingExpiration = memoryCacheItem.SlidingExpiration;

                    if (null != evictionCallback)
                    {
                        entry.RegisterPostEvictionCallback((object key, object value, EvictionReason reason, object state) =>
                        {
                            evictionCallback.Invoke(Convert.ToString(key), value, (ICache)state);
                        }, this);
                    }

                    return data;
                });
            }
            finally
            {
                this.Logger.LogInformation($"Item '{key}' returned from in-memory cache.");
                this.Lock.ExitReadLock();
            }
        }

        public void NotifyChanged(string dependencyMoniker)
        {
            if (string.IsNullOrWhiteSpace(dependencyMoniker)) return;

            this.Logger.LogTrace($"Waiting to notify change token '{dependencyMoniker}'.");

            this.Lock.EnterWriteLock();

            try
            {
                this.Logger.LogTrace($"Trying to retrieve change token '{dependencyMoniker}'.");

                if (this.MemoryCache.TryGetValue<DependencyChangeToken>(dependencyMoniker, out DependencyChangeToken changeToken))
                {
                    this.MemoryCache.Remove(dependencyMoniker);
                    changeToken.NotifyChanged();
                    this.Logger.LogInformation($"Change token '{dependencyMoniker}' notified.");
                }
            }
            finally
            {
                this.Lock.ExitWriteLock();
                this.Logger.LogTrace($"Change token '{dependencyMoniker}' expired.");
            }
        }

        public Task NotifyChangedAsync(string dependencyMoniker)
        {
            return Task.Run(() => this.NotifyChanged(dependencyMoniker));
        }

        #region IDisposable Support
        private bool IsDisposed = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!this.IsDisposed)
            {
                if (disposing)
                {
                    this.MemoryCache?.Dispose();
                    this.Lock.Dispose();
                }

                this.IsDisposed = true;
            }
            else
                throw new ObjectDisposedException(nameof(InMemoryCache));
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        #endregion
    }
}