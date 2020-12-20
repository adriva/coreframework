using System;
using Adriva.Storage.Abstractions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class StorageExtensions
    {
        public static IStorageBuilder AddStorage(this IServiceCollection services)
        {
            return services.AddStorage((serviceProvider) => new DefaultQueueMessageSerializer());
        }

        public static IStorageBuilder AddStorage(this IServiceCollection services, Func<IServiceProvider, IQueueMessageSerializer> queueMessageSerializerFactory)
        {
            services.AddSingleton<IQueueMessageSerializer>(queueMessageSerializerFactory);
            services.AddSingleton<IStorageClientFactory>(serviceProvider => ActivatorUtilities.CreateInstance<DefaultStorageClientFactory>(serviceProvider));
            return new DefaultStorageBuilder(services);
        }
    }
}