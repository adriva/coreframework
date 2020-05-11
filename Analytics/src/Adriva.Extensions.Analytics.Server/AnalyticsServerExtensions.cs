using System;
using Adriva.Extensions.Analytics.Server;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Provides extension methods to use Microsoft AppInsights server side services.
    /// </summary>
    public static class AnalyticsServerExtensions
    {
        /// <summary>
        /// Adds services required to host an analytics server to the service collection. 
        /// </summary>
        /// <param name="services">The Microsoft.Extensions.DependencyInjection.IServiceCollection to add services to.</param>
        /// <param name="build">Analytics server builder delegate.</param>
        /// <returns>The Microsoft.Extensions.DependencyInjection.IServiceCollection so that additional calls can be chained.</returns>
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

        /// <summary>
        /// Adds analytics server to the Http pipeline of a web application.
        /// </summary>
        /// <param name="app">The Microsoft.AspNetCore.Builder.IApplicationBuilder instance that the analytics server will be added to.</param>
        /// <param name="basePath">Base path of the analytics server to capture incoming requests.</param>
        /// <returns>The Microsoft.AspNetCore.Builder.IApplicationBuilder so that additional calls can be chained.</returns>
        public static IApplicationBuilder UseAnalyticsServer(this IApplicationBuilder app, PathString basePath)
        {
            if (string.IsNullOrWhiteSpace(basePath)) basePath = "/analytics";

            app.Map(basePath.Add("/track"), appBuilder => appBuilder.UseMiddleware<AnalyticsTrackingMiddleware>());

            return app;
        }
    }
}
