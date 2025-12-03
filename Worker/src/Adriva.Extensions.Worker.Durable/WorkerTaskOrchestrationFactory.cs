#if WTOF
using DurableTask.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Adriva.Extensions.Worker.Durable;

public sealed class WorkerTaskOrchestrationFactory : ObjectCreator<TaskOrchestration>
{
    private readonly IServiceProvider ServiceProvider;

    public WorkerTaskOrchestrationFactory(IServiceProvider serviceProvider)
    {
        this.ServiceProvider = serviceProvider;
    }

    public override TaskOrchestration Create()
    {
        throw new NotImplementedException();
    }
}
#endif