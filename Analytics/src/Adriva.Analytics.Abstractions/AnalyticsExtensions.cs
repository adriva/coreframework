using System;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.Extensions.DependencyInjection;

namespace Adriva.Analytics.Abstractions
{
    public static class AnalyticsExtensions
    {
        //TODO: https://docs.microsoft.com/en-us/azure/azure-monitor/app/ilogger#create-filter-rules-in-code

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
            services.AddSingleton<ITelemetryChannel, PersistentChannel>();
            return services;
        }
    }
}
