using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Adriva.Storage.Abstractions
{
    internal class DefaultStorageClientFactory : IStorageClientFactory
    {
        private readonly IServiceProvider ServiceProvider;
        private readonly StorageClientRegistry ClientRegistry;
        private ConcurrentDictionary<string, Lazy<Task<IStorageClient>>> LazyActivators;

        public DefaultStorageClientFactory(IServiceProvider serviceProvider)
        {
            this.ServiceProvider = serviceProvider;
            this.LazyActivators = new ConcurrentDictionary<string, Lazy<Task<IStorageClient>>>();
            this.ClientRegistry = this.ServiceProvider.GetRequiredService<IOptions<StorageClientRegistry>>().Value;
        }

        private Task<IStorageClient> GetStorageClientAsync(string qualifiedName)
        {
            var lazyActivator = this.LazyActivators.GetOrAdd(qualifiedName, x =>
            {
                if (!this.ClientRegistry.TryTakeClientActivator(qualifiedName, out Func<IServiceProvider, Lazy<Task<IStorageClient>>> lazyInitializer))
                {
                    throw new ArgumentException($"Storage client is not registered.");
                }

                return lazyInitializer(this.ServiceProvider);
            });

            return lazyActivator.Value;
        }

        public Task<IBlobClient> GetBlobClientAsync()
        {
            return this.GetBlobClientAsync(Options.DefaultName);
        }

        public async Task<IBlobClient> GetBlobClientAsync(string name)
        {
            string qualifiedName = Helpers.GetQualifiedBlobName(name);
            return (IBlobClient)await this.GetStorageClientAsync(qualifiedName);
        }

        public Task<IQueueClient> GetQueueClientAsync()
        {
            return this.GetQueueClientAsync(Options.DefaultName);
        }

        public async Task<IQueueClient> GetQueueClientAsync(string name)
        {
            string qualifiedName = Helpers.GetQualifiedQueueName(name);
            return (IQueueClient)await this.GetStorageClientAsync(qualifiedName);
        }

        public Task<ITableClient> GetTableClientAsync()
        {
            return this.GetTableClientAsync(Options.DefaultName);
        }

        public async Task<ITableClient> GetTableClientAsync(string name)
        {
            string qualifiedName = Helpers.GetQualifiedTableName(name);
            return (ITableClient)await this.GetStorageClientAsync(qualifiedName);
        }
    }
}