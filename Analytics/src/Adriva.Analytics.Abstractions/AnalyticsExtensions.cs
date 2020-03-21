using System;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.ApplicationInsights;

namespace Adriva.Analytics.Abstractions
{
    public static class AnalyticsExtensions
    {
        public static IServiceCollection AddAnalytics(this IServiceCollection services, Action<AnalyticsOptions> configure)
        {
            AnalyticsOptions analyticsOptions = new AnalyticsOptions();
            configure.Invoke(analyticsOptions);

            services.Configure<AnalyticsOptions>(baseOptions =>
            {
                baseOptions.BacklogSize = analyticsOptions.BacklogSize;
                baseOptions.Capacity = analyticsOptions.Capacity;
                baseOptions.EndPointAddress = analyticsOptions.EndPointAddress;
                baseOptions.InstrumentationKey = analyticsOptions.InstrumentationKey;
                baseOptions.IsDeveloperMode = analyticsOptions.IsDeveloperMode;
                baseOptions.Filter = analyticsOptions.Filter;
            });

            services.AddApplicationInsightsTelemetryWorkerService(options =>
            {
                options.InstrumentationKey = analyticsOptions.InstrumentationKey;
                options.DeveloperMode = analyticsOptions.IsDeveloperMode;
                options.EndpointAddress = analyticsOptions.EndPointAddress;
            });

            services.AddHttpClient();
            services.AddSingleton<ITelemetryChannel, PersistentChannel>();

            services.AddLogging(builder =>
            {
                foreach (var logLevelEntry in analyticsOptions.LogLevels)
                {
                    builder.AddFilter<ApplicationInsightsLoggerProvider>(logLevelEntry.Key, logLevelEntry.Value);
                }
            });

            return services;
        }
    }
}
