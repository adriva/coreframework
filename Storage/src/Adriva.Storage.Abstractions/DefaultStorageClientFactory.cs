using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

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
            throw new System.NotImplementedException();
        }

        public async Task<IBlobClient> GetBlobClientAsync(string name)
        {
            string queueName = Helpers.GetQualifiedBlobName(name);
            return (IBlobClient)await this.GetStorageClientAsync(queueName, name);
        }

        public Task<IQueueClient> GetQueueClientAsync()
        {
            throw new System.NotImplementedException();
        }

        public async Task<IQueueClient> GetQueueClientAsync(string name)
        {
            string queueName = Helpers.GetQualifiedQueueName(name);
            return (IQueueClient)await this.GetStorageClientAsync(queueName, name);
        }

        public Task<ITableClient> GetTableClientAsync()
        {
            throw new System.NotImplementedException();
        }

        public Task<ITableClient> GetTableClientAsync(string name)
        {
            throw new System.NotImplementedException();
        }
    }
}