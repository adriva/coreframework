using System;
using System.IO;
using System.Threading.Tasks;
using Adriva.Common.Core;

namespace Adriva.Storage.Abstractions
{
    public interface IBlobClient : IStorageClient
    {
        ValueTask<bool> ExistsAsync(string name);

        Task<BlobItemProperties> GetPropertiesAsync(string name);

        Task<Uri> UpsertAsync(string name, Stream stream, bool shouldOverwrite = false, int cacheDuration = 0);

        Task<Uri> UpsertAsync(string name, ReadOnlyMemory<byte> data, bool shouldOverwrite = false, int cacheDuration = 0);

        Task<Stream> OpenReadStreamAsync(string name);

        ValueTask DeleteAsync(string name);

        Task<SegmentedResult<string>> ListAllAsync(string continuationToken, string prefix = null, int count = 500);
    }
}