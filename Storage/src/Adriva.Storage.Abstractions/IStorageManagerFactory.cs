using System.Threading.Tasks;

namespace Adriva.Storage.Abstractions
{

    public interface IStorageManagerFactory
    {

        Task<IQueueManager> GetQueueManagerAsync();

        Task<IQueueManager> GetQueueManagerAsync(string name);
    }
}