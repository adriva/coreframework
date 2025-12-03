using System;
using System.Threading.Tasks;

namespace Adriva.Extensions.Faster
{
    public interface IFasterStorageClient
    {
        Task<StorageDataEntry> GetAsync(string key, bool isUserKey);

        ValueTask<bool> DeleteAsync(string key);

        ValueTask<string> UpsertAsync(string key, object data, string etag, TimeSpan? expireAfter);

        Task<FasterLockToken> TryAcquireLockAsync(string key, TimeSpan autoReleaseTimespan);

        ValueTask<bool> ReleaseLockAsync(FasterLockToken token);

        ValueTask PersistAsync();
    }
}