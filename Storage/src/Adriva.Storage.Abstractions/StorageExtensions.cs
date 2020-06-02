using Adriva.Storage.Abstractions;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class StorageExtensions
    {
        public static IStorageBuilder AddStorage(this IServiceCollection services)
        {
            services.AddOptions();
            services.TryAddSingleton<IStorageManagerFactory, DefaultStorageManagerFactory>();

            return new DefaultStorageBuilder(services);
        }
    }
}