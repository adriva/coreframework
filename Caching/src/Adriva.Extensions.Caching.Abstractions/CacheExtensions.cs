using Adriva.Extensions.Caching.Abstractions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class CacheExtensions
    {
        public static IServiceCollection AddCache<TCache>(this IServiceCollection services) where TCache : class, ICache
        {
            return services.AddSingleton<ICache, TCache>();
        }
    }
}
