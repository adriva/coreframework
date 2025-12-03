using System;
using System.Threading;
using System.Threading.Tasks;
using Adriva.Extensions.Worker.Abstractions;
using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Adriva.Extensions.Worker.Hangfire;

public sealed class HangfireWorkerHost : IWorkerHost
{
    private readonly IServiceProvider ServiceProvider;
    private readonly BackgroundJobServerOptions Options;
    private BackgroundJobServer JobServer;

    public HangfireWorkerHost(IServiceProvider serviceProvider, IOptions<BackgroundJobServerOptions> optionsAccessor)
    {
        this.ServiceProvider = serviceProvider;
        this.Options = optionsAccessor.Value;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        this.Options.Activator = this.ServiceProvider.GetService<JobActivator>() ?? JobActivator.Current;
        this.JobServer = new BackgroundJobServer(this.Options);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        this.JobServer?.SendStop();
        return this.JobServer?.WaitForShutdownAsync(cancellationToken);
    }
}
