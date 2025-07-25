using System;

namespace Adriva.Extensions.Caching.Abstractions
{
    public sealed class NullCacheItem : ICacheItem
    {
        public DateTimeOffset? AbsoluteExpiration
        {
            get => null;
            set { }
        }

        public TimeSpan? AbsoluteExpirationRelativeToNow
        {
            get => null;
            set { }
        }

        public TimeSpan? SlidingExpiration
        {
            get => null;
            set { }
        }
    }
}
