using Adriva.Extensions.Caching.Abstractions;
using Microsoft.Extensions.Caching.Distributed;

namespace Adriva.Extensions.Caching.SqlServer
{
    /// <inheritdoc />
    internal class SqlCacheItem : DistributedCacheEntryOptions, ICacheItem
    {

    }
}