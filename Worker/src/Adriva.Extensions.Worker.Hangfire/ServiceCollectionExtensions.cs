using System;
using Adriva.Extensions.Worker;
using Adriva.Extensions.Worker.Hangfire;
using Hangfire;
using Hangfire.Common;
using Hangfire.SqlServer;
using Newtonsoft.Json;

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

        public static IServiceCollection AddHangfire(this IServiceCollection services, HangfireOptions options, Action<JsonSerializerSettings> configureSerializer = null)
        {
            var globalConfiguration = GlobalConfiguration.Configuration;

            SqlServerStorageOptions sqlServerStorageOptions = new SqlServerStorageOptions()
            {
                SchemaName = string.IsNullOrWhiteSpace(options.SchemaName) ? "rt" : options.SchemaName,
                PrepareSchemaIfNecessary = true,
                UseRecommendedIsolationLevel = true,
                QueuePollInterval = TimeSpan.FromSeconds(60)
            };

            globalConfiguration.UseSqlServerStorage(options.ConnectionString, sqlServerStorageOptions);
            globalConfiguration.UseColouredConsoleLogProvider();

            options.AutomaticRetryCount = Math.Max(0, options.AutomaticRetryCount);

            globalConfiguration.UseFilter<AutomaticRetryAttribute>(new AutomaticRetryAttribute()
            {
                Attempts = options.AutomaticRetryCount
            });

            if (null != configureSerializer)
            {
                JsonSerializerSettings defaultJsonSerializerSettings = new JsonSerializerSettings();
                configureSerializer(defaultJsonSerializerSettings);
                globalConfiguration.UseSerializerSettings(defaultJsonSerializerSettings);
            }
            else
            {
                globalConfiguration.UseRecommendedSerializerSettings();
            }

            return services;
        }

        public static IServiceCollection AddStandaloneHangfireServer(this IServiceCollection services)
        {
            return services.AddStandaloneHangfireServer<HangfireJobActivator>();
        }

        public static IServiceCollection AddStandaloneHangfireServer<TJobActivator>(this IServiceCollection services)
        {
            return services
                    .AddSingleton<JobActivator, HangfireJobActivator>(serviceProvider => new HangfireJobActivator(serviceProvider, services))
                    .AddHostedService<HangfireServer>();
        }
    }
}
