namespace Adriva.Storage.Abstractions
{
    public interface IStorageBuilder
    {
        IStorageBuilder AddQueueClient<T>(bool isSingleton = false) where T : class, IQueueClient;

        IStorageBuilder AddQueueClient<T>(string name, bool isSingleton = false) where T : class, IQueueClient;
    }
}