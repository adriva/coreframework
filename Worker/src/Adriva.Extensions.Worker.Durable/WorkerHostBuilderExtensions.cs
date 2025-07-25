using Adriva.Extensions.Worker.Abstractions;
using DurableTask.Core;
using DurableTask.SqlServer;
using Microsoft.Extensions.DependencyInjection;

namespace Adriva.Extensions.Worker.Durable;

public static class WorkerHostBuilderExtensions
{
    public static ISqlServerDurableWorkerHostBuilder UseSqlServerDurableHost(this IWorkerHostBuilder builder, string connectionString, string appName, string schema)
    {

        builder.Services.AddSingleton<IWorkerHost, DurableWorkerHost>();
        builder.Services.AddSingleton<INameVersionObjectManager<TaskOrchestration>, WorkerNameVersionObjectManager<TaskOrchestration>>();
        builder.Services.AddSingleton<INameVersionObjectManager<TaskActivity>, WorkerNameVersionObjectManager<TaskActivity>>();
        // builder.Services.AddSingleton<WorkerTaskOrchestrationFactory>();
        builder.Services.AddSingleton(sp => (IWorkerClient)sp.GetRequiredService<IWorkerHost>());

        var platformSettings = new SqlOrchestrationServiceSettings(connectionString, appName, schema)
        {
            AppName = appName
        };

        builder.Services.AddSingleton(platformSettings);

        SqlServerDurableWorkerHostBuilder sqlServerDurableWorkerHostBuilder = new(builder.Services, platformSettings);
        return sqlServerDurableWorkerHostBuilder;
    }
}