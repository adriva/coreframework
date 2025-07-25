using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using DurableTask.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Adriva.Extensions.Worker.Durable;

public sealed class WorkerTaskOrchestration : TaskOrchestration<bool, object[]>
{
    public string Name { get; }

    public string Version { get; }

    public MethodInfo? Method { get; }

    public WorkerTaskOrchestration(string name, MethodInfo method)
    {
        this.Name = name;
        this.Version = "1.0";
        this.Method = method;
    }

    public override async Task<bool> RunTask(OrchestrationContext context, object[] input)
    {
        await Task.CompletedTask;
        throw new Exception("Some fault");
        await Task.CompletedTask;
        return true;
        // using IServiceScope scope = this.ServiceProvider.CreateScope();

        // TContainer containerInstance = ActivatorUtilities.CreateInstance<TContainer>(this.ServiceProvider);

        // try
        // {

        //     return true;
        // }
        // catch
        // {
        //     return false;
        // }
        // finally
        // {
        //     if (containerInstance is IDisposable disposableContainerInstance)
        //     {
        //         disposableContainerInstance.Dispose();
        //     }
        // }
    }
}
