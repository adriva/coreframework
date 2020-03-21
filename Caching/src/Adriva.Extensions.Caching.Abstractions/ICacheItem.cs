using System;

namespace Adriva.Extensions.Caching.Abstractions
{
    public interface ICacheItem
    {
        DateTimeOffset? AbsoluteExpiration { get; set; }

        TimeSpan? AbsoluteExpirationRelativeToNow { get; set; }

        TimeSpan? SlidingExpiration { get; set; }
    }
}
