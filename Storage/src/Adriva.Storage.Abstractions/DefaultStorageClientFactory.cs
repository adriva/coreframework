using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Adriva.Storage.Abstractions
{
    internal class DefaultStorageClientFactory : IStorageClientFactory
    {
        private readonly IServiceProvider ServiceProvider;

        public DefaultStorageClientFactory(IServiceProvider serviceProvider)
        {
            this.ServiceProvider = serviceProvider;
        }

        private async Task<IStorageClient> GetStorageClientAsync(string qualifiedName, string name)
        {
            var wrapperService = this.ServiceProvider.GetServices<StorageClientWrapper>().FirstOrDefault(wrapper => 0 == string.Compare(qualifiedName, wrapper.Name, StringComparison.OrdinalIgnoreCase));
            if (null == wrapperService) return null;
            StorageClientContext context = new StorageClientContext(this.ServiceProvider, qualifiedName, name);
            await wrapperService.InitializeAsync(context);
            return wrapperService.StorageClient;
        }

        public Task<IBlobClient> GetBlobClientAsync()
        {
            return this.GetBlobClientAsync(Options.DefaultName);
        }

        public async Task<IBlobClient> GetBlobClientAsync(string name)
        {
            string qualifiedName = Helpers.GetQualifiedBlobName(name);
            return (IBlobClient)await this.GetStorageClientAsync(qualifiedName, name);
        }

        public Task<IQueueClient> GetQueueClientAsync()
        {
            return this.GetQueueClientAsync(Options.DefaultName);
        }

        public async Task<IQueueClient> GetQueueClientAsync(string name)
        {
            string qualifiedName = Helpers.GetQualifiedQueueName(name);
            return (IQueueClient)await this.GetStorageClientAsync(qualifiedName, name);
        }

        public Task<ITableClient> GetTableClientAsync()
        {
            return this.GetTableClientAsync(Options.DefaultName);
        }

        public Task<ITableClient> GetTableClientAsync(string name)
        {
            throw new System.NotImplementedException();
        }
    }
}