using Adriva.Storage.Abstractions;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class StorageExtensions
    {
        public static IStorageBuilder AddStorage(this IServiceCollection services)
        {
            services.AddOptions();
            services.TryAddSingleton<IStorageClientFactory, DefaultStorageClientFactory>();

            return new DefaultStorageBuilder(services);
        }
    }
}