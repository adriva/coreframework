using System;
using Adriva.Extensions.Optimization.Abstractions;
using Adriva.Extensions.Optimization.Transforms;
using Adriva.Extensions.Optimization.Web;
using Microsoft.AspNetCore.Builder;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class OptimizationExtensions
    {
        public static IOptimizationBuilder<WebOptimizationOptions> AddOptimization(this IServiceCollection services, Action<WebOptimizationOptions> configure)
        {
            services.AddHttpContextAccessor();
            services.AddSingleton<OptimizationMiddleware>();
            services.AddScoped<IOptimizationContext, WebOptimizationContext>();
            services.AddSingleton<IOptimizationEvents<WebOptimizationOptions>, WebOptimizationEvents>();
            services.AddSingleton<IOptimizationResultTagBuilderFactory, OptimizationResultTagBuilderFactory>();
            return services
                .AddOptimization<WebOptimizationOptions>(options =>
                {
                    configure?.Invoke(options);
                })
                .AddTransformationChain("js", typeof(JavascriptBundleTransform), typeof(JavascriptMinificationTransform))
                .AddTransformationChain("css", typeof(StylesheetBundleTransform), typeof(StylesheetMinificationTransform))
                .AddTagBuilder<JavascriptResultTagBuilder>()
                .AddTagBuilder<StylesheetResultTagBuilder>()
            ;
        }

        public static IOptimizationBuilder<WebOptimizationOptions> AddTagBuilder<TBuilder>(this IOptimizationBuilder<WebOptimizationOptions> builder) where TBuilder : class, IOptimizationResultTagBuilder
        {
            builder.Services.AddSingleton<IOptimizationResultTagBuilder, TBuilder>();
            return builder;
        }

        public static IApplicationBuilder UseOptimization(this IApplicationBuilder app)
        {
            app.UseMiddleware<OptimizationMiddleware>();
            return app;
        }
    }
}
