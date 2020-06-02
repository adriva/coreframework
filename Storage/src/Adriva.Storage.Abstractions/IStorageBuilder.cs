namespace Adriva.Storage.Abstractions
{
    public interface IStorageBuilder
    {
        IStorageBuilder AddQueueManager<T>(string name) where T : class, IQueueManager;
    }
}