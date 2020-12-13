using System;
using Adriva.Extensions.Caching.Abstractions;
using Newtonsoft.Json;

namespace Adriva.Extensions.Caching.Distributed
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    internal class DistributedCacheEntry : ICacheItem
    {
        public DateTimeOffset? AbsoluteExpiration { get; set; }

        public TimeSpan? AbsoluteExpirationRelativeToNow { get; set; }

        public TimeSpan? SlidingExpiration { get; set; }

        [JsonProperty]
        public long Timestamp { get; set; }

        [JsonProperty]
        public string Data { get; set; }
    }
}
