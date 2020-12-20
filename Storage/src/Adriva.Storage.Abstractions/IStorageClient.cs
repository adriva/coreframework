using System;
using System.Threading.Tasks;

namespace Adriva.Storage.Abstractions
{
    public interface IStorageClient : IAsyncDisposable
    {
        ValueTask InitializeAsync(StorageClientContext context);
    }
}