using System;
using Adriva.Extensions.Caching.Abstractions;
using Microsoft.Extensions.Caching.Memory;

namespace Adriva.Extensions.Caching.Memory
{
    internal class MemoryCacheItem : ICacheItem
    {
        private readonly ICacheEntry CacheEntry;

        public DateTimeOffset? AbsoluteExpiration
        {
            get => this.CacheEntry.AbsoluteExpiration;
            set => this.CacheEntry.AbsoluteExpiration = value;
        }

        public TimeSpan? AbsoluteExpirationRelativeToNow
        {
            get => this.CacheEntry.AbsoluteExpirationRelativeToNow;
            set => this.CacheEntry.AbsoluteExpirationRelativeToNow = value;
        }

        public TimeSpan? SlidingExpiration
        {
            get => this.CacheEntry.SlidingExpiration;
            set => this.CacheEntry.SlidingExpiration = value;
        }

        internal MemoryCacheItem(ICacheEntry cacheEntry)
        {
            this.CacheEntry = cacheEntry;
        }
    }
}