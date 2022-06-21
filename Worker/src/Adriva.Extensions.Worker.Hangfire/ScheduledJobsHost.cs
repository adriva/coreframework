using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Adriva.Common.Core;
using Hangfire;
using Hangfire.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Adriva.Extensions.Worker.Hangfire
{
    public partial class ScheduledJobsHost : ScheduledJobsHostBase
    {
        private BackgroundJobServer JobServer;

        public ScheduledJobsHost(IServiceProvider serviceProvider) : base(serviceProvider)
        {

        }

        private bool CheckCronExpression(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression))
            {
                return false;
            }

            try
            {
                _ = Cronos.CronExpression.Parse(expression, Cronos.CronFormat.IncludeSeconds);
                return true;
            }
            catch
            {
                this.Logger.LogError($"Given expression '{expression}' could not be parsed by the CronExpressionParser.");
                return false;
            }
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            var providers = this.ServiceProvider.GetRequiredService<IJobFilterProvider>() as JobFilterProviderCollection;
            providers.Add(ActivatorUtilities.CreateInstance<DefaultFilterProvider>(this.ServiceProvider));

            this.JobServer = new BackgroundJobServer(new BackgroundJobServerOptions()
            {
                ServerName = $"{Environment.MachineName}",
                Activator = this.ServiceProvider.GetRequiredService<JobActivator>(),
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
                    try
                    {
                        BackgroundJob.Enqueue(() => this.RunWithHangfireAsync(scheduledItemInstance));
                    }
                    catch (Exception enqueueError)
                    {
                        this.Logger.LogError(enqueueError, $"Error enqueueing start-up job '{scheduledItem.JobId}'. This error will be ignored.");
                    }
                }

                if (scheduledItem.Parser is CronExpressionParser && this.CheckCronExpression(scheduledItem.Expression))
                {
                    RecurringJob.AddOrUpdate(scheduledItem.JobId, () => this.RunWithHangfireAsync(scheduledItemInstance), scheduledItem.Expression);
                }
            }

            await this.JobServer.WaitForShutdownAsync(stoppingToken);
        }
    }
}
