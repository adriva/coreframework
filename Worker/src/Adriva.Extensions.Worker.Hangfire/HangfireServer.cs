using System;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Adriva.Extensions.Worker.Hangfire
{
    public sealed class HangfireServer : IHostedService
    {
        private readonly IServiceProvider ServiceProvider;
        private BackgroundJobServer JobServer;

        public HangfireServer(IServiceProvider serviceProvider)
        {
            this.ServiceProvider = serviceProvider;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            this.JobServer = new BackgroundJobServer(new BackgroundJobServerOptions()
            {
                ServerName = $"{Environment.MachineName}",
                Activator = this.ServiceProvider.GetRequiredService<JobActivator>(),
                WorkerCount = 1
            });

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            this.JobServer?.SendStop();
            return this.JobServer?.WaitForShutdownAsync(cancellationToken);
        }
    }
}
