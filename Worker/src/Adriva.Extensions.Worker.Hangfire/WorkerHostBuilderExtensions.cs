using System;
using System.Linq;
using Adriva.Extensions.Worker.Abstractions;
using Adriva.Extensions.Worker.Hangfire;
using Hangfire;
using Hangfire.SqlServer;

namespace Microsoft.Extensions.DependencyInjection;

public static class WorkerHostBuilderExtensions
{
    public static ISqlServerHangfireHostBuilder UseSqlServerHangfireHost(this IWorkerHostBuilder builder, string connectionString, string schema)
    {
        var globalConfiguration = GlobalConfiguration.Configuration;

        SqlServerStorageOptions sqlServerStorageOptions = new()
        {
            SchemaName = string.IsNullOrWhiteSpace(schema) ? "hfworker" : schema,
            PrepareSchemaIfNecessary = true,
            UseRecommendedIsolationLevel = true,
            QueuePollInterval = TimeSpan.FromSeconds(60)
        };

        globalConfiguration.UseSqlServerStorage(connectionString, sqlServerStorageOptions);

        builder.Services.AddSingleton<IWorkerHost, HangfireWorkerHost>();

        builder.UseSqlServerHangfireClient(connectionString, schema);

        return new SqlServerHangfireHostBuilder(builder.Services);
    }

    public static IWorkerHostBuilder UseSqlServerHangfireClient(this IWorkerHostBuilder builder, string connectionString, string schema)
    {
        if (!builder.Services.Any(x => typeof(IBackgroundJobClientFactory).IsAssignableFrom(x.ServiceType)))
        {
            builder.Services.AddHangfire(x =>
            {
                SqlServerStorageOptions sqlServerStorageOptions = new()
                {
                    SchemaName = string.IsNullOrWhiteSpace(schema) ? "hfworker" : schema,
                    PrepareSchemaIfNecessary = true,
                    UseRecommendedIsolationLevel = true,
                    QueuePollInterval = TimeSpan.FromSeconds(60)
                };

                x.UseSqlServerStorage(connectionString, sqlServerStorageOptions);
            });

            builder.Services.AddSingleton<IWorkerClient, HangfireWorkerClient>();
        }

        return builder;
    }
}

public static class ServiceCollectionExtensions
{
    // public static IServiceCollection AddHangfireScheduledJobs(this IServiceCollection services)
    // {
    //     return services
    //         .AddSingleton<JobActivator, HangfireJobActivator>(serviceProvider => new HangfireJobActivator(serviceProvider, services))
    //         .AddSingleton<IJobFilterProvider>(serviceProvider => JobFilterProviders.Providers)
    //         .AddScheduledJobs<ScheduledJobsHost>();
    // }

    // public static IServiceCollection AddHangfire(this IServiceCollection services, HangfireOptions options, Action<JsonSerializerSettings> configureSerializer = null)
    // {
    //     var globalConfiguration = GlobalConfiguration.Configuration;

    //     SqlServerStorageOptions sqlServerStorageOptions = new SqlServerStorageOptions()
    //     {
    //         SchemaName = string.IsNullOrWhiteSpace(options.SchemaName) ? "rt" : options.SchemaName,
    //         PrepareSchemaIfNecessary = true,
    //         UseRecommendedIsolationLevel = true,
    //         QueuePollInterval = TimeSpan.FromSeconds(60)
    //     };

    //     globalConfiguration.UseSqlServerStorage(options.ConnectionString, sqlServerStorageOptions);
    //     globalConfiguration.UseColouredConsoleLogProvider();

    //     options.AutomaticRetryCount = Math.Max(0, options.AutomaticRetryCount);

    //     globalConfiguration.UseFilter<AutomaticRetryAttribute>(new AutomaticRetryAttribute()
    //     {
    //         Attempts = options.AutomaticRetryCount
    //     });

    //     if (null != configureSerializer)
    //     {
    //         JsonSerializerSettings defaultJsonSerializerSettings = new JsonSerializerSettings();
    //         configureSerializer(defaultJsonSerializerSettings);
    //         globalConfiguration.UseSerializerSettings(defaultJsonSerializerSettings);
    //     }
    //     else
    //     {
    //         globalConfiguration.UseRecommendedSerializerSettings();
    //     }

    //     return services;
    // }

    // public static IServiceCollection AddStandaloneHangfireServer(this IServiceCollection services)
    // {
    //     return services.AddStandaloneHangfireServer<HangfireJobActivator>();
    // }

    // public static IServiceCollection AddStandaloneHangfireServer<TJobActivator>(this IServiceCollection services)
    // {
    //     return services
    //             .AddSingleton<JobActivator, HangfireJobActivator>(serviceProvider => new HangfireJobActivator(serviceProvider, services))
    //             .AddHostedService<HangfireServer>();
    // }
}
