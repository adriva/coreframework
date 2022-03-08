using Adriva.Extensions.Caching.Abstractions;
using Microsoft.Extensions.Caching.Distributed;

namespace Adriva.Extensions.Caching.SqlServer
{
    /// <inheritdoc />
    public class SqlCacheItem : DistributedCacheEntryOptions, ICacheItem
    {

    }
}