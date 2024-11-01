using System;
using Hangfire.Client;
using Hangfire.Common;

namespace Adriva.Extensions.Worker.Hangfire
{
    /// <summary>
    /// Cancels the processing of a recurring job in case it is scheduled by the system to compensate for a missed scheduled and maximum delay (in seconds) have passed.
    /// <remarks>Only applies to recurring (scheduled) jobs.</remarks>
    /// </summary>
    public sealed class IgnoreMissedRunAttribute : JobFilterAttribute, IClientFilter
    {
        private readonly int MaximumDelayInSeconds;

        public IgnoreMissedRunAttribute(int maximumDelayInSeconds)
        {
            this.MaximumDelayInSeconds = Math.Max(0, maximumDelayInSeconds);
        }

        public void OnCreating(CreatingContext filterContext)
        {
            if (filterContext.Parameters.TryGetValue(HangfireDefaults.RecurringJobIdKey, out var recurringJobId) /*&& filterContext.InitialState?.Reason == "Triggered by recurring job scheduler"*/)
            {
                var recurringJob = filterContext.Connection.GetAllEntriesFromHash($"{HangfireDefaults.RecurringJobEntryPrefix}:{recurringJobId}");

                if (recurringJob != null && recurringJob.TryGetValue(HangfireDefaults.NextExecutionKey, out var nextExecution))
                {
                    // the next execution time of a recurring job is updated AFTER the job instance creation,
                    // so at the moment it still contains the scheduled execution time from the previous run.
                    var scheduledTime = JobHelper.DeserializeDateTime(nextExecution);

                    if (scheduledTime.AddSeconds(this.MaximumDelayInSeconds) < DateTime.UtcNow)
                    {
                        filterContext.Canceled = true;
                    }
                }
            }
        }

        public void OnCreated(CreatedContext filterContext)
        {

        }
    }
}
