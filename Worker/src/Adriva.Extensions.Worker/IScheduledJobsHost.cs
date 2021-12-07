using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Adriva.Extensions.Worker
{
    public interface IScheduledJobsHost
    {
        IEnumerable<MethodInfo> ResolveScheduledMethods();

        ValueTask<LockStatus> RunAsync(MethodInfo methodInfo);
    }
}
