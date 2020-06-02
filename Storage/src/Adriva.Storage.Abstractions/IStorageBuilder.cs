using Microsoft.Extensions.DependencyInjection;

namespace Adriva.Storage.Abstractions
{
    public interface IStorageBuilder
    {
        IServiceCollection Services { get; }

        IStorageClientBuilder AddQueueClient<T>(bool isSingleton = false) where T : class, IQueueClient;

        IStorageClientBuilder AddQueueClient<T>(string name, bool isSingleton = false) where T : class, IQueueClient;

        IStorageClientBuilder AddBlobClient<T>(bool isSingleton = false) where T : class, IBlobClient;

        IStorageClientBuilder AddBlobClient<T>(string name, bool isSingleton = false) where T : class, IBlobClient;
    }
}