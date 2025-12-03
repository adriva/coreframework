using System.Reflection;
using Adriva.Common.Core;
using DurableTask.Core;

namespace Adriva.Extensions.Worker.Durable;

public sealed class WorkerNameVersionObjectManager<T> : INameVersionObjectManager<T> where T : class
{
    public void Add(ObjectCreator<T> creator)
    {
        System.Console.WriteLine("OK");
    }

    public T? GetObject(string name, string? version)
    {
        MethodInfo? orchestratorMethod;

        try
        {
            orchestratorMethod = Helpers.FindMethod(name, ClrMemberFlags.Instance | ClrMemberFlags.Static | ClrMemberFlags.Public);
        }
        catch
        {
            orchestratorMethod = null;
        }

        if (orchestratorMethod is null)
        {
            return default;
        }

        return new WorkerTaskOrchestration(name, orchestratorMethod) as T;
    }
}
