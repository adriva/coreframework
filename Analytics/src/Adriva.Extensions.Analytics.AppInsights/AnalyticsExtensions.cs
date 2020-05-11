using System;
using Adriva.Extensions.Analytics.AppInsights;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.ApplicationInsights;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Provides extension methods to use Microsoft AppInsights wrapper services.
    /// </summary>
    public static class AnalyticsExtensions
    {
        private static IServiceCollection AddAppInsightsAnalytics(this IServiceCollection services, Action<IAnalyticsBuilder> build)
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

        /// <summary>
        /// Adds Microsoft AppInsights for asp.net core services to the specified service collection.
        /// </summary>
        /// <param name="services">The Microsoft.Extensions.DependencyInjection.IServiceCollection to add services to.</param>
        /// <param name="configure">The AnalyticsOptions configuration delegate.</param>
        /// <returns>The Microsoft.Extensions.DependencyInjection.IServiceCollection so that additional calls can be chained.</returns>
        public static IServiceCollection AddAppInsightsWebAnalytics(this IServiceCollection services, Action<AnalyticsOptions> configure)
        {
            services.AddAppInsightsAnalytics(builder =>
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

        /// <summary>
        /// Adds Microsoft AppInsights for generic host core services to the specified service collection.
        /// </summary>
        /// <param name="services">The Microsoft.Extensions.DependencyInjection.IServiceCollection to add services to.</param>
        /// <param name="configure">The AnalyticsOptions configuration delegate.</param>
        /// <returns>The Microsoft.Extensions.DependencyInjection.IServiceCollection so that additional calls can be chained.</returns>
        public static IServiceCollection AddAppInsightsGenericAnalytics(this IServiceCollection services, Action<AnalyticsOptions> configure)
        {

            services.AddAppInsightsAnalytics(builder =>
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
