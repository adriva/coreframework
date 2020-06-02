using System;
using System.IO;
using System.Threading.Tasks;
using Adriva.Storage.Abstractions;
using Microsoft.Extensions.Options;

namespace Adriva.Storage.Azure
{

    public class AzureBlobClient : IBlobClient
    {
        private readonly IOptionsMonitor<AzureBlobConfiguration> ConfigurationAccessor;

        private AzureBlobConfiguration Configuration;

        public AzureBlobClient(IOptionsMonitor<AzureBlobConfiguration> configuration)
        {
            this.ConfigurationAccessor = configuration;
        }

        public ValueTask InitializeAsync(string name)
        {
            this.Configuration = this.ConfigurationAccessor.Get(name);
            return new ValueTask();
        }

        public ValueTask<bool> ExistsAsync(string name)
        {
            throw new NotImplementedException();
        }

        public Task<Stream> OpenReadStreamAsync(string name)
        {
            throw new NotImplementedException();
        }

        public Task<ReadOnlyMemory<byte>> ReadAllBytesAsync(string name)
        {
            throw new NotImplementedException();
        }

        public Task<string> UpsertAsync(string name, Stream stream, int cacheDuration = 0)
        {
            throw new NotImplementedException();
        }

        public Task<string> UpsertAsync(string name, ReadOnlySpan<byte> data, int cacheDuration = 0)
        {
            throw new NotImplementedException();
        }

        public ValueTask DeleteAsync(string name)
        {
            throw new NotImplementedException();
        }

        public ValueTask DisposeAsync()
        {
            throw new NotImplementedException();
        }
    }
}
