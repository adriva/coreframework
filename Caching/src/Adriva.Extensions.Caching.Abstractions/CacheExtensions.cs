using Microsoft.Extensions.DependencyInjection;

namespace Adriva.Extensions.Caching.Abstractions
{
    public static class CacheExtensions
    {
        public static IServiceCollection AddCache<TCache>(this IServiceCollection services) where TCache : class, ICache
        {
            return services.AddSingleton<ICache, TCache>();
        }
    }
}
