using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Eventing.Reader;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Adriva.Extensions.Worker.Abstractions;
using DurableTask.Core;
using DurableTask.SqlServer;
using Microsoft.Extensions.DependencyInjection;

namespace Adriva.Extensions.Worker.Durable;

public sealed class DurableWorkerHost : IWorkerHost, IWorkerClient
{
    private readonly SqlOrchestrationService SqlOrchestrationService;
    // private readonly WorkerTaskOrchestrationFactory TaskOrchestrationFactory;
    private readonly TaskHubWorker TaskHubWorker;
    private readonly IServiceProvider ServiceProvider;

    private TaskHubClient? TaskHubClient;

    public DurableWorkerHost(IServiceProvider serviceProvider, SqlOrchestrationServiceSettings settings, INameVersionObjectManager<TaskOrchestration> taskOrchestrationObjectManager, INameVersionObjectManager<TaskActivity> taskActivityObjectManager)
    {
        this.ServiceProvider = serviceProvider;

        this.SqlOrchestrationService = new(settings);
        this.TaskHubWorker = new(this.SqlOrchestrationService, taskOrchestrationObjectManager, taskActivityObjectManager);

        // this.TaskHubWorker.AddTaskOrchestrations(this.TaskOrchestrationFactory);
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
#if DEBUG
        await this.SqlOrchestrationService.DeleteAsync();
#endif

        await this.SqlOrchestrationService.CreateIfNotExistsAsync();

        this.TaskHubWorker.AddOrchestrationDispatcherMiddleware(async (context, next) =>
        {
            // string name = context.GetProperty<DurableTask.Core.OrchestrationRuntimeState>().Name;
            await next();
        });

        await this.TaskHubWorker.StartAsync();

        this.TaskHubClient = new(this.SqlOrchestrationService);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (CancellationToken.None == cancellationToken)
        {
            await this.TaskHubWorker.StopAsync(false);
        }
        else
        {
            TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.None);

            cancellationToken.Register(async () =>
            {
                await this.TaskHubWorker.StopAsync(false);
                taskCompletionSource.TrySetResult();
            });

            await taskCompletionSource.Task;
        }
    }

    public async Task<string> Enqueue<TContainer>(Expression<Func<TContainer, Task>> expression)
    {
        Debug.Assert(this.TaskHubClient is not null);

        var orchestration = DurableExpressionVisitor<TContainer>.GetOrchestration(expression);

        var instance = await this.TaskHubClient.CreateOrchestrationInstanceAsync(orchestration.Name, orchestration.Version, null);
        // await this.TaskHubClient.ResumeInstanceAsync(instance);
        await this.TaskHubClient.RaiseEventAsync(instance, "StartCustom", new());
        // await this.TaskHubClient.WaitForOrchestrationAsync(instance, TimeSpan.FromSeconds(50));

        // System.Console.WriteLine(instance);
        return instance.ExecutionId;
    }
}
