using System.Threading.Tasks;

namespace Adriva.Storage.Abstractions
{

    public interface IStorageClientFactory
    {

        Task<IQueueClient> GetQueueClientAsync();

        Task<IQueueClient> GetQueueClientAsync(string name);
    }
}