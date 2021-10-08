using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Adriva.Extensions.Worker
{
    public interface IScheduledJobsHost
    {
        IEnumerable<MethodInfo> ResolveScheduledMethods();

        Task<string> RunJobAsync(MethodInfo methodInfo);
    }
}
