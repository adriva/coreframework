using System;
using System.Threading.Tasks;

namespace Adriva.Extensions.Worker
{
    internal sealed class NullLock : IWorkerLock
    {
        public ValueTask<LockStatus> AcquireLockAsync(string jobId, string instanceId, TimeSpan timeout)
        {
            return new ValueTask<LockStatus>(new LockStatus(instanceId, true));
        }

        public ValueTask ReleaseLockAsync(string jobId, string instanceId)
        {
            return new ValueTask();
        }
    }
}
