using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Adriva.Extensions.Worker
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddScheduledJobs(this IServiceCollection services)
        {
            return services.AddScheduledJobs<ScheduledJobsHost>();
        }

        public static IServiceCollection AddScheduledJobs<THost>(this IServiceCollection services) where THost : class, IScheduledJobsHost, IHostedService
        {
            if (services.Any(s => s.ServiceType.Equals(typeof(IScheduledJobsHost))))
            {
                throw new InvalidOperationException($"Another scheduled job host has already been added. Only one scheduled job host is allowed per service container.");
            }

            services.AddSingleton<IScheduledJobsHost, THost>();
            services.AddHostedService<THost>(serviceProvider =>
            {
                var instance = serviceProvider.GetService<IScheduledJobsHost>();
                return instance as THost;
            });
            return services;
        }
    }
}
