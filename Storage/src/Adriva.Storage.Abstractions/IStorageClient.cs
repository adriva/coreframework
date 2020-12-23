using System;
using System.Threading.Tasks;

namespace Adriva.Storage.Abstractions
{
    public interface IStorageClient : IAsyncDisposable, IDisposable
    {
        ValueTask InitializeAsync(StorageClientContext context);
    }
}