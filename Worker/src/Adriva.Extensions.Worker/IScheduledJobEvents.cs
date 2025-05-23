using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Adriva.Extensions.Worker
{
    public interface IScheduledJobEvents
    {
        Task ExecutingAsync(object owner, string instanceId, MethodInfo methodInfo);

        Task ExecutedAsync(object owner, string instanceId, MethodInfo methodInfo, Exception error);
    }
}
