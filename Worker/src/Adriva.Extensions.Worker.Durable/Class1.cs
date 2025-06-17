using DurableTask.Core;
using DurableTask.SqlServer;

namespace Adriva.Extensions.Worker.Durable;

public sealed class Container
{
    public class AllOrch : TaskOrchestration
    {
        public override async Task<string> Execute(OrchestrationContext context, string input)
        {
            await context.ScheduleTask<WriteTaskActivity>("WName", "1.0", input);
            return "null";
        }

        public override string GetStatus()
        {
            return "none";
        }

        public override void RaiseEvent(OrchestrationContext context, string name, string input)
        {
            System.Console.WriteLine(name);
        }
    }

    public class WriteTaskActivity : TaskActivity
    {
        public override string Run(TaskContext context, string input)
        {
            System.Console.WriteLine("dd");
            return string.Empty;
        }

        public override Task<string> RunAsync(TaskContext context, string input)
        {
            return base.RunAsync(context, input);
        }
    }
}

public interface IDurableWorkerHost
{

}

public sealed class DurableWorkerHost : IDurableWorkerHost
{
    private readonly SqlOrchestrationService SqlOrchestrationService;
    private readonly TaskHubWorker TaskHubWorker;

    public DurableWorkerHost()
    {
        SqlOrchestrationServiceSettings settings = new("Server=10.255.1.127\\SQL2017,1435;Database=Portal_UAT;User Id=poasPortal;Password=portal12;MultipleActiveResultSets=True;Encrypt=False", "testhub", "dtx");
        this.SqlOrchestrationService = new(settings);

        this.TaskHubWorker = new(this.SqlOrchestrationService);
        this.TaskHubWorker.AddTaskActivities([new Container.WriteTaskActivity()]);
        this.TaskHubWorker.AddTaskOrchestrations([typeof(Container.AllOrch)]);
    }

    public async Task StartAsync()
    {
        await this.SqlOrchestrationService.CreateIfNotExistsAsync();


        this.TaskHubWorker.AddOrchestrationDispatcherMiddleware(async (context, next) =>
       {
           await next();
       });

        await this.TaskHubWorker.StartAsync();

        TaskHubClient client = new(this.SqlOrchestrationService);
        // var oi = await client.CreateOrchestrationInstanceAsync(typeof(Container.AllOrch), "input1");
        var h = await client.GetOrchestrationHistoryAsync(new()
        {
            ExecutionId = "0e800266515440fa8b1327ed9586a720",
            InstanceId = "6c7fb9df74d64871b3c3e39826a753a8"
        });
        // await client.TerminateInstanceAsync(new()
        // {
        //     ExecutionId = "07112d8f610e47cca82a6f0dfe55c9eb",
        //     InstanceId = "9d3aa6ceab62422e88d139f2115e3ac7"
        // });
        var ii = DurableTask.Core.Serializing.JsonDataConverter.Default.Deserialize<DurableTask.Core.History.HistoryEvent[]>(h);
    }
}