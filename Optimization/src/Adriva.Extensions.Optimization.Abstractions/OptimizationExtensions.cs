using System;
using Adriva.Extensions.Optimization.Abstractions;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class OptimizationExtensions
    {
        public static IOptimizationBuilder<OptimizationOptions> AddOptimization(this IServiceCollection services, Action<OptimizationOptions> configure)
        {
            return services.AddOptimization<OptimizationOptions>(configure);
        }

        public static IOptimizationBuilder<TOptions> AddOptimization<TOptions>(this IServiceCollection services, Action<TOptions> configure) where TOptions : OptimizationOptions, new()
        {
            IOptimizationBuilder<TOptions> builder = new OptimizationBuilder<TOptions>(services);
            services.AddHttpClient();
            services.TryAddSingleton<IOptimizationManager, DefaultOptimizationManager<TOptions>>();
            services.TryAddScoped<IOptimizationScope, DefaultOptimizationScope>();
            services.TryAddTransient<IOptimizationContext, DefaultOptimizationContext>();
            services.AddInMemoryCache();
            services.Configure<TOptions>((options) =>
            {
                options.AddLoader<PhysicalFileAssetLoader>();
                options.AddLoader<HttpAssetLoader>();
                configure.Invoke(options);
            });
            return builder;
        }
    }
}