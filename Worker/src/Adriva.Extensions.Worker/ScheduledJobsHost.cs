using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Adriva.Common.Core;
using Microsoft.Extensions.Logging;

namespace Adriva.Extensions.Worker
{
    internal class ScheduledJobsHost : ScheduledJobsHostBase
    {
        private readonly Timer Timer;

        private DateTime LastRunDate;
        private long RunningItemCount = 0;

        public ScheduledJobsHost(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            this.Timer = new Timer(this.OnTimerElapsed, null, Timeout.Infinite, 5000);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await base.ExecuteAsync(stoppingToken);

            this.LastRunDate = DateTime.UtcNow;
            foreach (var scheduledItem in this.ScheduledItems)
            {
                DateTime? nextRunDate = scheduledItem.Parser.GetNext(this.LastRunDate, scheduledItem.Expression);

                if (!nextRunDate.HasValue) continue;

                this.Logger.LogInformation($"Next scheduled run for '{scheduledItem.Method.Name}' is at '{nextRunDate.Value}'.");
            }
            while (0 != DateTime.Now.Second % 5)
            {
                await Task.Delay(250);
            }
            this.Timer.Change(0, 5000);
        }

        private void OnTimerElapsed(object state)
        {
            foreach (var scheduledItem in this.ScheduledItems)
            {
                DateTime? nextRunDate = null;
                lock (scheduledItem)
                {
                    if (scheduledItem.ShouldRunOnStartup) nextRunDate = DateTime.MinValue;
                    scheduledItem.ShouldRunOnStartup = false;
                }

                nextRunDate = nextRunDate ?? scheduledItem.Parser.GetNext(this.LastRunDate, scheduledItem.Expression);

                if (!nextRunDate.HasValue) continue;

                if (!scheduledItem.IsQueued && nextRunDate.Value <= DateTime.Now)
                {
                    lock (scheduledItem)
                    {
                        if (!scheduledItem.IsQueued && nextRunDate.Value <= DateTime.Now)
                        {
                            scheduledItem.IsQueued = true;
                            ScheduledItemInstance scheduledItemInstance = new ScheduledItemInstance(scheduledItem, Guid.NewGuid().ToString());
                            _ = this.RunItemAsync(scheduledItemInstance);
                        }
                    }
                }
            }
        }

        private async void SafeRunItemAsync(object state)
        {
            ScheduledItem scheduledItem = null;
            ScheduledItemInstance scheduledItemInstance = null;

            try
            {
                scheduledItemInstance = state as ScheduledItemInstance;
                scheduledItem = scheduledItemInstance.ScheduledItem;

                if (null == scheduledItem) return;

                if (!scheduledItem.IsRunning)
                {
                    lock (scheduledItem)
                    {
                        if (!scheduledItem.IsRunning)
                        {
                            scheduledItem.IsRunning = true;
                        }
                        else return;
                    }
                }

                Interlocked.Increment(ref this.RunningItemCount);

                await this.RunItemAsync(scheduledItemInstance);
            }
            catch (Exception fatalError)
            {
                this.Logger.LogError(fatalError, $"Failed to execute scheduled job '{scheduledItem.Method.Name}'.");
            }
            finally
            {
                this.LastRunDate = DateTime.UtcNow;
                scheduledItem.IsRunning = false;
                scheduledItem.IsQueued = false;
                Interlocked.Decrement(ref this.RunningItemCount);
                DateTime? nextRunDate = scheduledItem.Parser.GetNext(this.LastRunDate, scheduledItem.Expression);
                if (nextRunDate.HasValue)
                {
                    this.Logger.LogInformation($"Next scheduled run for '{scheduledItem.Method.Name}' is at '{nextRunDate.Value}'.");
                }
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            this.Timer.Change(Timeout.Infinite, Timeout.Infinite);
            this.CancellationTokenSource.Cancel();

            await base.StopAsync(cancellationToken);

            SpinWait.SpinUntil(() =>
            {
                return 0 == Interlocked.Read(ref this.RunningItemCount);
            }, 30000);
        }

        public override void Dispose()
        {
            if (!this.IsDisposed)
            {
                base.Dispose();
                this.Timer.Dispose();
            }
        }
    }
}
