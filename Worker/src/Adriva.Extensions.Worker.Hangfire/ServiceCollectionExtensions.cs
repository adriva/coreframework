using Adriva.Extensions.Worker;
using Adriva.Extensions.Worker.Hangfire;
using Hangfire;
using Hangfire.Common;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddHangfireScheduledJobs(this IServiceCollection services)
        {
            return services
                .AddSingleton<JobActivator, HangfireJobActivator>(serviceProvider => new HangfireJobActivator(serviceProvider, services))
                .AddSingleton<IJobFilterProvider>(serviceProvider => JobFilterProviders.Providers)
                .AddScheduledJobs<ScheduledJobsHost>();
        }
    }
}
