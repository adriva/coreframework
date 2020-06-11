using System.Threading.Tasks;

namespace Adriva.Storage.Abstractions
{
    public interface IStorageClientFactory
    {
        Task<IQueueClient> GetQueueClientAsync();

        Task<IQueueClient> GetQueueClientAsync(string name);

        Task<IBlobClient> GetBlobClientAsync();

        Task<IBlobClient> GetBlobClientAsync(string name);

        Task<ITableClient> GetTableClientAsync();

        Task<ITableClient> GetTableClientAsync(string name);
    }
}