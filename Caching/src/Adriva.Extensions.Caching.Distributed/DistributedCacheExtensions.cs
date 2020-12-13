using Adriva.Extensions.Caching.Distributed;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DistributedCacheExtensions
    {
        /// <summary>
        /// Adds the distributed cache service in the services collection.
        /// </summary>
        /// <remarks>To use the distributed cache services a concrete implementation of IDistributedCache interface must be added to the services collection.</remarks>
        /// <param name="services">The Microsoft.Extensions.DependencyInjection.IServiceCollection to add the service to.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static IServiceCollection AddDistributedCache(this IServiceCollection services)
        {
            return services.AddCache<DistributedCache>();
        }
    }
}