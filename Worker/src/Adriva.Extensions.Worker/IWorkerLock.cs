using System;
using System.Threading.Tasks;

namespace Adriva.Extensions.Worker
{
    public interface IWorkerLock
    {
        ValueTask<LockStatus> AcquireLockAsync(string jobId, string instanceId, TimeSpan timeout);

        ValueTask ReleaseLockAsync(string jobId, string instanceId);
    }
}
