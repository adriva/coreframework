using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Adriva.Extensions.Worker
{
    public interface IScheduledJobEvents
    {
        Task ExecutingAsync(string instanceId, MethodInfo methodInfo);

        Task ExecutedAsync(string instanceId, MethodInfo methodInfo, Exception error);
    }
}
