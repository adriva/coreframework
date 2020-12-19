using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Adriva.Storage.Abstractions
{
    public interface IStorageClientFactory
    {
        Task<IQueueClient> GetQueueClientAsync();

        Task<IQueueClient> GetQueueClientAsync(string name);

        Task<IBlobClient> GetBlobClientAsync();

        Task<IBlobClient> GetBlobClientAsync(string name);

        Task<ITableClient> GetTableClientAsync();

        Task<ITableClient> GetTableClientAsync(string name);
    }

    internal class DefaultStorageClientFactory : IStorageClientFactory
    {
        private readonly IServiceProvider ServiceProvider;

        public DefaultStorageClientFactory(IServiceProvider serviceProvider)
        {
            this.ServiceProvider = serviceProvider;
        }

        public Task<IBlobClient> GetBlobClientAsync()
        {
            throw new System.NotImplementedException();
        }

        public Task<IBlobClient> GetBlobClientAsync(string name)
        {
            throw new System.NotImplementedException();
        }

        public Task<IQueueClient> GetQueueClientAsync()
        {
            throw new System.NotImplementedException();
        }

        public async Task<IQueueClient> GetQueueClientAsync(string name)
        {
            name = Helpers.GetQueueName(name);
            var wrapperService = this.ServiceProvider.GetServices<StorageClientWrapper>().FirstOrDefault(wrapper => 0 == string.Compare(name, wrapper.Name, StringComparison.OrdinalIgnoreCase));
            if (null == wrapperService) return null;
            await wrapperService.InitializeAsync();
            return (IQueueClient)wrapperService.StorageClient;
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