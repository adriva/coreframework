using System;
using Microsoft.Extensions.DependencyInjection;

namespace Adriva.Extensions.Worker
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddScheduledJobs(this IServiceCollection services)
        {
            services.AddSingleton<IScheduledJobsHost, ScheduledJobsHost>();
            services.AddHostedService<ScheduledJobsHost>(serviceProvider =>
            {
                var instance = serviceProvider.GetRequiredService<IScheduledJobsHost>();

                if (null == instance)
                {
                    throw new InvalidProgramException("IScheduledJobsHost implementation is overriden. Please do not inject custom services implementing the IScheduledJobHost interface.");
                }

                return instance as ScheduledJobsHost;
            });
            return services;
        }
    }
}
