using System;
using Adriva.Extensions.Worker;
using Adriva.Extensions.Worker.Hangfire;
using Hangfire;
using Hangfire.SqlServer;
using Newtonsoft.Json;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class WorkerHostBuilderExtensions
    {
        public static IWorkerHostBuilder UseHangfire(this IWorkerHostBuilder builder, HangfireOptions options, Action<JsonSerializerSettings> configureSerializer = null)
        {
            builder.HostBuilder.ConfigureServices((context, services) =>
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
            });
            return builder;
        }
    }
}
