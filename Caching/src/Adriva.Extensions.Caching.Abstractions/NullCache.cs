using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Adriva.Extensions.Caching.Abstractions
{
    public sealed class NullCache : ICache
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<T> GetOrCreateAsync<T>(string key, Func<ICacheItem, Task<T>> factory, EvictionCallback evictionCallback = null, params string[] dependencyMonikers)
        {
            NullCacheItem nullCacheItem = new NullCacheItem();
            return factory.Invoke(nullCacheItem);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task<T> GetOrCreateAsync<T>(string key, Func<ICacheItem, Task<T>> factory, params string[] dependencyMonikers)
                 => this.GetOrCreateAsync<T>(key, factory, null, dependencyMonikers);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void NotifyChanged(string key, string dependencyMoniker)
        {

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Task NotifyChangedAsync(string key, string dependencyMoniker) => Task.CompletedTask;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ValueTask RemoveAsync(string key) => new ValueTask();
    }
}
