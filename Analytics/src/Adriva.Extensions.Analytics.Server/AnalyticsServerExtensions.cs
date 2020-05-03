using System;
using Microsoft.AspNetCore.Builder;

namespace Microsoft.Extensions.DependencyInjection
{

    public static class AnalyticsServerExtensions
    {
        public static IServiceCollection AddAnalyticsServer(this IServiceCollection services, Action<AnalyticsServerOptions> configure)
        {
            services.Configure(configure);
            return services;
        }

        public static IApplicationBuilder UseAnalyticsServer(this IApplicationBuilder app, Action<IAnalyticsServerBuilder> build)
        {
            AnalyticsServerBuilder analyticsBuilder = ActivatorUtilities.CreateInstance<AnalyticsServerBuilder>(app.ApplicationServices);
            build?.Invoke(analyticsBuilder);
            analyticsBuilder.Build(app);
            return app;
        }
    }
}
