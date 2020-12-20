using Adriva.Storage.Abstractions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class StorageExtensions
    {
        public static IStorageBuilder AddStorage(this IServiceCollection services)
        {
            services.AddSingleton<IStorageClientFactory>(serviceProvider => ActivatorUtilities.CreateInstance<DefaultStorageClientFactory>(serviceProvider));
            return new DefaultStorageBuilder(services);
        }
    }
}