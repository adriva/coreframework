namespace Adriva.Storage.Abstractions
{
    public interface IStorageBuilder
    {
        IStorageBuilder AddQueueManager<T>();

        IStorageBuilder AddQueueManager<T>(string name);
    }
}