using System;
using Adriva.Extensions.Analytics.Server;
using Adriva.Extensions.Analytics.Server.AppInsights;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class AppInsightsAnalyticsServerExtensions
    {
        public static IServiceCollection AddAppInsightsAnalyticsServer(this IServiceCollection services, Action<IAnalyticsServerBuilder> build)
        {
            services.AddAnalyticsServer(builder =>
            {
                builder.UseHandler<AppInsightsHandler>();
                build.Invoke(builder);
            });

            services.AddSingleton<AnalyticsItemPopulator, RequestItemPopulator>();
            services.AddSingleton<AnalyticsItemPopulator, MetricItemPopulator>();
            services.AddSingleton<AnalyticsItemPopulator, DependencyItemPopulator>();
            services.AddSingleton<AnalyticsItemPopulator, ExceptionItemPopulator>();
            services.AddSingleton<AnalyticsItemPopulator, TraceItemPopulator>();
            services.AddSingleton<AnalyticsItemPopulator, EventItemPopulator>();
            services.AddSingleton<AnalyticsItemPopulator, AvailabilityItemPopulator>();

            services.AddSingleton<IAppInsightsValidator, AppInsightsValidator>();

            return services;
        }
    }
}