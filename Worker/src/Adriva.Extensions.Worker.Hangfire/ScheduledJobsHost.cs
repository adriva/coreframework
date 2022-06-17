using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Adriva.Common.Core;
using Hangfire;
using Hf = Hangfire;

namespace Adriva.Extensions.Worker.Hangfire
{
    public class ScheduledJobsHost : ScheduledJobsHostBase
    {
        private sealed class HangfireJobActivatorScope : Hf.JobActivatorScope
        {
            private readonly JobActivatorScope Scope;

            public HangfireJobActivatorScope(JobActivatorScope scope)
            {
                this.Scope = scope;
            }

            public override object Resolve(Type type) => this.Scope.Resolve(type);

            public override void DisposeScope()
            {
                // base.DisposeScope();
            }
        }

        private sealed class HangfireJobActivator : Hf.JobActivator
        {
            private readonly ScheduledJobsHost Host;

            public override object ActivateJob(Type jobType)
            {
                return this.Host;
            }

            public HangfireJobActivator(ScheduledJobsHost host)
            {
                this.Host = host;
            }

            public override JobActivatorScope BeginScope(JobActivatorContext context)
            {
                return new HangfireJobActivatorScope(base.BeginScope(context));
            }
        }

        private Hf.BackgroundJobServer JobServer;

        public ScheduledJobsHost(IServiceProvider serviceProvider) : base(serviceProvider)
        {

        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            this.JobServer = new Hf.BackgroundJobServer(new Hf.BackgroundJobServerOptions()
            {
                ServerName = $"{Environment.MachineName}",
                Activator = new ScheduledJobsHost.HangfireJobActivator(this),
                WorkerCount = 1
            });
            await base.StartAsync(cancellationToken);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            this.JobServer.SendStop();
            return this.JobServer.WaitForShutdownAsync(cancellationToken);
        }

        public async Task RunWithHangfireAsync(ScheduledItemInstance scheduledItemInstance)
        {
            if (string.IsNullOrWhiteSpace(scheduledItemInstance.InstanceId) || string.IsNullOrWhiteSpace(scheduledItemInstance.ScheduledItem?.JobId))
            {
                return;
            }

            var matchingItem = this.ScheduledItems.FirstOrDefault(s => 0 == string.Compare(s.JobId, scheduledItemInstance.ScheduledItem.JobId));

            if (null == matchingItem)
            {
                return;
            }

            await this.RunItemAsync(new ScheduledItemInstance(matchingItem, scheduledItemInstance.InstanceId));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await base.ExecuteAsync(stoppingToken);
            foreach (var scheduledItem in this.ScheduledItems)
            {
                ScheduledItemInstance scheduledItemInstance = new ScheduledItemInstance(scheduledItem, Guid.NewGuid().ToString());

                if (scheduledItem.ShouldRunOnStartup)
                {
                    BackgroundJob.Enqueue(() => this.RunWithHangfireAsync(scheduledItemInstance));
                }

                RecurringJob.AddOrUpdate(scheduledItem.JobId, () => this.RunWithHangfireAsync(scheduledItemInstance), scheduledItem.Expression);
            }

            await this.JobServer.WaitForShutdownAsync(stoppingToken);
        }
    }
}
