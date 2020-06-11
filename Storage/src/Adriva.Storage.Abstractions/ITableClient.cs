using System.Threading.Tasks;

namespace Adriva.Storage.Abstractions
{
    public interface ITableClient : IStorageClient
    {
        Task<TItem> GetAsync<TItem>(string partitionKey, string rowKey) where TItem : class;
    }
}