using Adriva.Extensions.Caching.Abstractions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class CacheExtensions
    {
        public static IServiceCollection AddCache<TCache>(this IServiceCollection services) where TCache : class, ICache
        {
            services.AddSingleton<TCache>();
            services.AddSingleton<ICache>(serviceProvider =>
            {
                return serviceProvider.GetRequiredService<TCache>();
            });
            services.AddSingleton<ICache<TCache>>(serviceProvider =>
            {
                var cacheInstance = serviceProvider.GetRequiredService<TCache>();
                var cacheWrapper = new CacheWrapper<TCache>(cacheInstance);
                return cacheWrapper;
            });
            return services;
        }
    }
}
