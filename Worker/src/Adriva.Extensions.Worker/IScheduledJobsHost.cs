using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Adriva.Extensions.Worker
{
    public interface IScheduledJobsHost
    {
        IEnumerable<MethodInfo> ResolveScheduledMethods();

        Task<LockStatus> RunAsync(MethodInfo methodInfo);

        bool TryResolveMethod(ScheduledItem scheduledItem, out MethodInfo methodInfo);
    }
}
