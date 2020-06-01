using System;
using Adriva.Extensions.Optimization.Abstractions;
using Adriva.Extensions.Optimization.Transforms;
using Adriva.Extensions.Optimization.Web;
using Microsoft.AspNetCore.Builder;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Provides extension methods to use Optimization services.
    /// </summary>
    public static class OptimizationExtensions
    {
        /// <summary>
        /// Adds the minimum required services to use the optimization system.
        /// <remarks>This method does not register any transformation chains or tag builders.await You need to manually add them using the AddTransformationChain and AddTagBuilder extension methods.</remarks>
        /// </summary>
        /// <param name="services">The Microsoft.Extensions.DependencyInjection.IServiceCollection to add services to.</param>
        /// <param name="configure">The WebOptimizationOptions configuration delegate.</param>
        /// <returns>The Adriva.Extensions.Optimization.Abstractions.IOptimizationBuilder so that additional calls can be chained.</returns>
        public static IOptimizationBuilder<WebOptimizationOptions> AddOptimizationCore(this IServiceCollection services, Action<WebOptimizationOptions> configure)
        {
            services.AddHttpContextAccessor();
            services.AddSingleton<OptimizationMiddleware>();
            services.AddSingleton<IOptimizationEvents<WebOptimizationOptions>, WebOptimizationEvents>();
            services.AddSingleton<IOptimizationResultTagBuilderFactory, OptimizationResultTagBuilderFactory>();
            return services
                .AddOptimization<WebOptimizationOptions, WebOptimizationContext>(options =>
                {
                    configure?.Invoke(options);
                });
        }

        /// <summary>
        /// Adds the default services to use the optimization system.
        /// <remarks>This method registers default transformation chains and tag builders for javascript and css files.</remarks>
        /// </summary>
        /// <param name="services">The Microsoft.Extensions.DependencyInjection.IServiceCollection to add services to.</param>
        /// <param name="configure">The WebOptimizationOptions configuration delegate.</param>
        /// <returns>The Adriva.Extensions.Optimization.Abstractions.IOptimizationBuilder so that additional calls can be chained.</returns>
        public static IOptimizationBuilder<WebOptimizationOptions> AddOptimization(this IServiceCollection services, Action<WebOptimizationOptions> configure)
        {
            return services.AddOptimizationCore(configure)
                .AddTransformationChain("js", typeof(JavascriptBundleTransform), typeof(JavascriptMinificationTransform))
                .AddTransformationChain("css", typeof(StylesheetBundleTransform), typeof(StylesheetMinificationTransform))
                .AddTagBuilder<JavascriptResultTagBuilder>()
                .AddTagBuilder<StylesheetResultTagBuilder>()
            ;
        }

        /// <summary>
        /// Adds a tag builder service to be used in optimization.
        /// </summary>
        /// <param name="builder">The Adriva.Extensions.Optimization.Abstractions.IOptimizationBuilder that the tag builder will be added to.</param>
        /// <typeparam name="TBuilder">The type of the concrete class implementing IOptimizationResultTagBuilder interface.</typeparam>
        /// <returns>The Adriva.Extensions.Optimization.Abstractions.IOptimizationBuilder so that additional calls can be chained.</returns>
        public static IOptimizationBuilder<WebOptimizationOptions> AddTagBuilder<TBuilder>(this IOptimizationBuilder<WebOptimizationOptions> builder)
                    where TBuilder : class, IOptimizationResultTagBuilder
        {
            builder.Services.AddSingleton<IOptimizationResultTagBuilder, TBuilder>();
            return builder;
        }

        /// <summary>
        /// Adds the Adriva.Extensions.Optimization.Web.OptimizationMiddleware to the specified Microsoft.AspNetCore.Builder.IApplicationBuilder, which enables asset optimization capabilities.
        /// </summary>
        /// <param name="app">The Microsoft.AspNetCore.Builder.IApplicationBuilder to add the middleware to.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static IApplicationBuilder UseOptimization(this IApplicationBuilder app)
        {
            app.UseMiddleware<OptimizationMiddleware>();
            return app;
        }
    }
}
