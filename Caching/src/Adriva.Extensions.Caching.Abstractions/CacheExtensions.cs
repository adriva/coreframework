using Adriva.Extensions.Caching.Abstractions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class CacheExtensions
    {
        public static IServiceCollection AddCache<TCache>(this IServiceCollection services) where TCache : class, ICache
        {
            var serviceProvider = services.BuildServiceProvider();
            TCache cacheInstance = ActivatorUtilities.CreateInstance<TCache>(serviceProvider);

            return services.AddSingleton<ICache<TCache>>(serviceProvider =>
            {
                services.AddSingleton<ICache>(cacheInstance);
                return new CacheWrapper<TCache>(cacheInstance);
            }).AddSingleton<ICache>(cacheInstance);
        }
    }
}
