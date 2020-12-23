using System.Threading.Tasks;

namespace Adriva.Storage.Abstractions
{
    public interface IStorageClient
    {
        ValueTask InitializeAsync(StorageClientContext context);
    }
}