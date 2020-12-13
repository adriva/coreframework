using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
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
        private readonly ConcurrentQueue<Tuple<Action<string>, string>> NotificationQueue = new ConcurrentQueue<Tuple<Action<string>, string>>();

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<T> GetOrCreateAsync<T>(string key, Func<ICacheItem, Task<T>> factory, params string[] dependencyMonikers)
        {
            return this.GetOrCreateAsync(key, factory, null, dependencyMonikers);
        }

        public async Task<T> GetOrCreateAsync<T>(string key, Func<ICacheItem, Task<T>> factory, EvictionCallback evictionCallback = null, params string[] dependencyMonikers)
        {
            while (0 < this.NotificationQueue.Count && this.NotificationQueue.TryDequeue(out Tuple<Action<string>, string> notifyTuple))
            {
                notifyTuple.Item1?.Invoke(notifyTuple.Item2);
            }

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

        public async ValueTask RemoveAsync(string key)
        {
            if (String.IsNullOrWhiteSpace(key)) return;

            this.MemoryCache.Remove(key);
            await Task.CompletedTask;
        }

        public void NotifyChanged(string dependencyMoniker)
        {
            if (string.IsNullOrWhiteSpace(dependencyMoniker)) return;

            Action<string> notifyAction = (moniker) =>
            {
                this.Logger.LogTrace($"Trying to retrieve change token '{moniker}'.");
                if (this.MemoryCache.TryGetValue<DependencyChangeToken>(moniker, out DependencyChangeToken changeToken))
                {
                    this.MemoryCache.Remove(moniker);
                    changeToken.NotifyChanged();
                    this.Logger.LogInformation($"Change token '{moniker}' notified.");
                }
            };

            this.NotificationQueue.Enqueue(new Tuple<Action<string>, string>(notifyAction, dependencyMoniker));
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