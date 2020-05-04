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
            });

            services.AddSingleton<AnalyticsItemPopulator, RequestItemPopulator>();
            services.AddSingleton<AnalyticsItemPopulator, MetricItemPopulator>();
            services.AddSingleton<AnalyticsItemPopulator, DependencyItemPopulator>();
            services.AddSingleton<AnalyticsItemPopulator, ExceptionItemPopulator>();
            return services;
        }
    }
}