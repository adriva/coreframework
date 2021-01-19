using Microsoft.Extensions.DependencyInjection;

namespace Adriva.Extensions.Worker
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddScheduledJobs(this IServiceCollection services)
        {
            services.AddHostedService<ScheduledJobsHost>();
            return services;
        }
    }
}
