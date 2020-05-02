using System;
using Adriva.Extensions.Analytics.AppInsights;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.ApplicationInsights;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class AnalyticsExtensions
    {
        private static IServiceCollection AddAnalytics(this IServiceCollection services, Action<IAnalyticsBuilder> build)
        {
            AnalyticsOptions analyticsOptions = new AnalyticsOptions();
            AnalyticsBuilder builder = new AnalyticsBuilder(services, analyticsOptions);
            build?.Invoke(builder);
            builder.ConfigureAction?.Invoke(analyticsOptions);

            services.Configure<AnalyticsOptions>(baseOptions =>
            {
                baseOptions.BacklogSize = analyticsOptions.BacklogSize;
                baseOptions.Capacity = analyticsOptions.Capacity;
                baseOptions.EndPointAddress = analyticsOptions.EndPointAddress;
                baseOptions.InstrumentationKey = analyticsOptions.InstrumentationKey;
                baseOptions.IsDeveloperMode = analyticsOptions.IsDeveloperMode;
                baseOptions.Filter = analyticsOptions.Filter;
            });

            services.Replace(ServiceDescriptor.Singleton<ITelemetryChannel, PersistentChannel>());

            services.AddLogging(builder =>
            {
                foreach (var logLevelEntry in analyticsOptions.LogLevels)
                {
                    builder.AddFilter<ApplicationInsightsLoggerProvider>(logLevelEntry.Key, logLevelEntry.Value);
                }
            });

            return services;
        }

        public static IServiceCollection AddWebAnalytics(this IServiceCollection services, Action<AnalyticsOptions> configure)
        {
            services.AddAnalytics(builder =>
            {
                builder.Configure(configure);

                services.AddApplicationInsightsTelemetry(options =>
                {
                    options.InstrumentationKey = builder.Options.InstrumentationKey;
                    options.DeveloperMode = builder.Options.IsDeveloperMode;
                    options.EndpointAddress = builder.Options.EndPointAddress;
                });
            });

            return services;
        }

        public static IServiceCollection AddGenericAnalytics(this IServiceCollection services, Action<AnalyticsOptions> configure)
        {

            services.AddAnalytics(builder =>
            {
                builder.Configure(configure);
                builder.Services.AddApplicationInsightsTelemetryWorkerService(options =>
                {
                    options.InstrumentationKey = builder.Options.InstrumentationKey;
                    options.DeveloperMode = builder.Options.IsDeveloperMode;
                    options.EndpointAddress = builder.Options.EndPointAddress;
                });
            });

            return services;
        }
    }
}
