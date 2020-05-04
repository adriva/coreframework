using System;
using Adriva.Extensions.Analytics.Server;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Microsoft.Extensions.DependencyInjection
{

    public static class AnalyticsServerExtensions
    {
        public static IServiceCollection AddAnalyticsServer(this IServiceCollection services, Action<IAnalyticsServerBuilder> build)
        {
            AnalyticsServerBuilder analyticsBuilder = new AnalyticsServerBuilder(services);
            analyticsBuilder.UseHandler<NullHandler>();

            build.Invoke(analyticsBuilder);
            analyticsBuilder.Build();
            services.AddSingleton<IQueueingService, QueueingService>();
            services.AddHostedService<QueueProcessorService>();
            return services;
        }

        public static IApplicationBuilder UseAnalyticsServer(this IApplicationBuilder app, PathString basePath)
        {
            if (string.IsNullOrWhiteSpace(basePath)) basePath = "/analytics";

            app.Map(basePath.Add("/track"), appBuilder => appBuilder.UseMiddleware<AnalyticsTrackingMiddleware>());

            return app;
        }
    }
}
