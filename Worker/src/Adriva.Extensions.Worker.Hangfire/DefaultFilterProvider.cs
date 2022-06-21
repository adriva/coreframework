using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Hangfire.Common;

namespace Adriva.Extensions.Worker.Hangfire
{
    public class DefaultFilterProvider : IJobFilterProvider
    {
        private readonly IServiceProvider ServiceProvider;
        private readonly IScheduledJobsHost ScheduledJobsHost;

        public DefaultFilterProvider(IServiceProvider serviceProvider, IScheduledJobsHost scheduledJobsHost)
        {
            this.ScheduledJobsHost = scheduledJobsHost;
            this.ServiceProvider = serviceProvider;
        }

        public IEnumerable<JobFilter> GetFilters(Job job)
        {
            var scheduledItemInstance = job.Args.OfType<ScheduledItemInstance>().FirstOrDefault();

            if (!this.ScheduledJobsHost.TryResolveMethod(scheduledItemInstance?.ScheduledItem, out MethodInfo methodInfo))
            {
                return Enumerable.Empty<JobFilter>();
            }

            return methodInfo.GetCustomAttributes<JobFilterAttribute>().Select(x => new JobFilterWrapper(this.ServiceProvider, x, JobFilterScope.Method, null));
        }
    }
}
