using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Adriva.Extensions.Worker
{
    internal class ScheduledJobsHost : ScheduledJobsHostBase
    {
        private readonly Timer Timer;

        private int TimerElapseCheck = 0;
        private long RunningItemCount = 0;

        public ScheduledJobsHost(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            this.Timer = new Timer(this.OnTimerElapsed, null, Timeout.Infinite, 5000);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await base.ExecuteAsync(stoppingToken);

            while (0 != DateTime.Now.Second % 5)
            {
                await Task.Delay(250);
            }
            this.Timer.Change(0, 5000);
        }

        private void OnTimerElapsed(object state)
        {
            if (1 == Interlocked.CompareExchange(ref this.TimerElapseCheck, 1, 0))
            {
                return;
            }

            try
            {
                foreach (var scheduledItem in this.ScheduledItems)
                {
                    lock (scheduledItem)
                    {
                        if (scheduledItem.ShouldRunOnStartup)
                        {
                            scheduledItem.NextScheduledDate = DateTime.Now;
                            scheduledItem.ShouldRunOnStartup = false;
                            scheduledItem.IsReadyToRun = true;
                        }
                        else if (!scheduledItem.IsReadyToRun)
                        {
                            scheduledItem.NextScheduledDate = scheduledItem.Parser.GetNext(scheduledItem.NextScheduledDate ?? DateTime.Now, scheduledItem.Expression);
                            scheduledItem.IsReadyToRun = true;
                            this.Logger.LogInformation($"Next scheduled run for '{scheduledItem.Method.Name}' is at '{scheduledItem.NextScheduledDate.Value}'.");
                        }
                    }

                    if (!scheduledItem.IsReadyToRun) continue;

                    if (scheduledItem.IsReadyToRun && scheduledItem.NextScheduledDate <= DateTime.Now)
                    {
                        lock (scheduledItem)
                        {
                            if (scheduledItem.IsReadyToRun && scheduledItem.NextScheduledDate <= DateTime.Now)
                            {
                                ScheduledItemInstance scheduledItemInstance = new ScheduledItemInstance(scheduledItem, Guid.NewGuid().ToString());

                                try
                                {
                                    _ = this.RunItemAsync(scheduledItemInstance);
                                }
                                finally
                                {
                                    scheduledItem.IsReadyToRun = false;
                                }
                            }
                        }
                    }
                }
            }
            finally
            {
                Interlocked.Exchange(ref this.TimerElapseCheck, 0);
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
