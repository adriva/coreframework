using DurableTask.SqlServer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Adriva.Extensions.Worker.Durable;

public interface ISqlServerDurableWorkerHostBuilder
{
    ISqlServerDurableWorkerHostBuilder WithPlatformOptions(Action<SqlOrchestrationServiceSettings> configure);
}

internal sealed class SqlServerDurableWorkerHostBuilder(IServiceCollection services, SqlOrchestrationServiceSettings platformSettings) : ISqlServerDurableWorkerHostBuilder
{
    private readonly IServiceCollection Services = services;
    private readonly SqlOrchestrationServiceSettings PlatformSettings = platformSettings;

    public ISqlServerDurableWorkerHostBuilder WithPlatformOptions(Action<SqlOrchestrationServiceSettings> configure)
    {
        configure(this.PlatformSettings);
        return this;
    }
}
