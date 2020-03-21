using Adriva.Extensions.Caching.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Adriva.Extensions.Caching.Memory
{
    public static class InMemoryCacheExtensions
    {
        /// <summary>
        /// Adds the in-memory cache service in the services collection.
        /// </summary>
        /// <param name="services">The Microsoft.Extensions.DependencyInjection.IServiceCollection to add the service to.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static IServiceCollection AddInMemoryCache(this IServiceCollection services)
        {
            return services.AddCache<InMemoryCache>();
        }
    }
}