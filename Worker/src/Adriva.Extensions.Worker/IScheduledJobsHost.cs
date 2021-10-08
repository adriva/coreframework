using System.Collections.Generic;
using System.Reflection;

namespace Adriva.Extensions.Worker
{
    public interface IScheduledJobsHost
    {
        IEnumerable<MethodInfo> ResolveScheduledMethods();

        string Run(MethodInfo methodInfo);
    }
}
