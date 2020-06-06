using System;
using Adriva.Storage.Abstractions;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class StorageExtensions
    {
        public static IServiceCollection AddStorage(this IServiceCollection services, Action<IStorageBuilder> build)
        {
            services.AddOptions();
            services.TryAddSingleton<IStorageClientFactory, DefaultStorageClientFactory>();

            if (null != build)
            {
                IStorageBuilder builder = new DefaultStorageBuilder(services);
                build.Invoke(builder);
            }

            return services;
        }
    }
}