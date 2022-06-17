using Adriva.Extensions.Worker;
using Adriva.Extensions.Worker.Hangfire;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddHangfireScheduledJobs(this IServiceCollection services)
        {
            return services.AddScheduledJobs<ScheduledJobsHost>();
        }
    }
}
