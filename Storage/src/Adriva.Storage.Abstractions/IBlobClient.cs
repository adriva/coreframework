using System;
using System.IO;
using System.Threading.Tasks;

namespace Adriva.Storage.Abstractions
{
    public interface IBlobClient : IStorageClient
    {
        ValueTask<bool> ExistsAsync(string name);

        Task<string> UpsertAsync(string name, Stream stream, int cacheDuration = 0);

        Task<string> UpsertAsync(string name, ReadOnlyMemory<byte> data, int cacheDuration = 0);

        Task<Stream> OpenReadStreamAsync(string name);

        Task<ReadOnlyMemory<byte>> ReadAllBytesAsync(string name);

        ValueTask DeleteAsync(string name);
    }
}