using Microsoft.Extensions.DependencyInjection;

namespace Adriva.Storage.Abstractions
{
    public interface IStorageBuilder
    {
        IServiceCollection Services { get; }

        IStorageClientBuilder AddQueueClient<T>(ServiceLifetime serviceLifetime = ServiceLifetime.Singleton) where T : class, IQueueClient;

        IStorageClientBuilder AddQueueClient<T>(string name, ServiceLifetime serviceLifetime = ServiceLifetime.Singleton) where T : class, IQueueClient;

        IStorageClientBuilder AddBlobClient<T>(ServiceLifetime serviceLifetime = ServiceLifetime.Singleton) where T : class, IBlobClient;

        IStorageClientBuilder AddBlobClient<T>(string name, ServiceLifetime serviceLifetime = ServiceLifetime.Singleton) where T : class, IBlobClient;

        IStorageClientBuilder AddTableClient<T>(ServiceLifetime serviceLifetime = ServiceLifetime.Singleton) where T : class, ITableClient;

        IStorageClientBuilder AddTableClient<T>(string name, ServiceLifetime serviceLifetime = ServiceLifetime.Singleton) where T : class, ITableClient;
    }
}