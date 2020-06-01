using System;
using System.Threading.Tasks;

namespace Adriva.Storage.Abstractions
{
    public interface IStorageManager : IAsyncDisposable
    {
        ValueTask InitializeAsync();
    }
}