using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;

namespace Adriva.Extensions.Caching.SqlServer
{
    internal interface IDatabaseOperations
    {
        byte[] GetCacheItem(string key);

        Task<byte[]> GetCacheItemAsync(string key, CancellationToken token = default(CancellationToken));

        void RefreshCacheItem(string key);

        Task RefreshCacheItemAsync(string key, CancellationToken token = default(CancellationToken));

        void DeleteCacheItem(string key);

        Task DeleteCacheItemAsync(string key, CancellationToken token = default(CancellationToken));

        void SetCacheItem(string key, byte[] value, DistributedCacheEntryOptions options);

        Task SetCacheItemAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default(CancellationToken));

        Task AddOrUpdateDependencyMonikersAsync(string key, string[] dependencyMonikers);

        void NotifyChanged(string key, string dependencyMoniker);

        Task NotifyChangedAsync(string key, string dependencyMoniker);

        void DeleteExpiredCacheItems();
    }
}