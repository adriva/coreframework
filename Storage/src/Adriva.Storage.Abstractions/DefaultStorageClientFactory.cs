using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Adriva.Storage.Abstractions
{
    internal sealed class DefaultStorageClientFactory : IStorageClientFactory, IDisposable
    {
        private sealed class StorageClientWrapper : IDisposable
        {
            private readonly IStorageClient StorageClient;
            private readonly string Name;
            private bool IsInitialized = false;
            private SemaphoreSlim InitializerSemaphore = new SemaphoreSlim(1, 1);

            public StorageClientWrapper(string name, IStorageClient storageClient)
            {
                this.StorageClient = storageClient;
                this.Name = name;
            }

            public async Task<IStorageClient> UnwrapAsync()
            {
                if (this.IsInitialized) return this.StorageClient;

                await this.InitializerSemaphore.WaitAsync();
                try
                {
                    if (!this.IsInitialized)
                    {
                        await this.StorageClient.InitializeAsync(this.Name);
                        this.IsInitialized = true;
                    }
                }
                finally
                {
                    this.InitializerSemaphore.Release();
                }

                return this.StorageClient;
            }

            public void Dispose()
            {
                this.InitializerSemaphore.Dispose();
            }
        }

        private readonly IServiceProvider ServiceProvider;
        private readonly IOptionsMonitor<StorageClientFactoryOptions> FactoryOptionsMonitor;
        private readonly ConcurrentDictionary<string, StorageClientWrapper> ReusableClients = new ConcurrentDictionary<string, StorageClientWrapper>();

        public DefaultStorageClientFactory(IServiceProvider serviceProvider,
            IOptionsMonitor<StorageClientFactoryOptions> factoryOptionsMonitor)
        {
            this.ServiceProvider = serviceProvider;
            this.FactoryOptionsMonitor = factoryOptionsMonitor;
        }

        private async Task<T> GetOrCreateStorageClientAsync<T>(string prefix, string name) where T : IStorageClient
        {
            if (null == name) throw new ArgumentNullException(nameof(name));

            name = string.Concat(prefix, ":", name);

            var options = this.FactoryOptionsMonitor.Get(name);

            if (null == options.ManagerType)
            {
                throw new InvalidOperationException($"No storage managers with the name '{name}' is registered.");
            }

            Func<T> factoryMethod = new Func<T>(() =>
            {
                return (T)ActivatorUtilities.CreateInstance(this.ServiceProvider, options.ManagerType);
            });

            if (ServiceLifetime.Singleton != options.ServiceLifetime)
            {
                T clientInstance = factoryMethod.Invoke();
                await clientInstance.InitializeAsync(name);
                return clientInstance;
            }
            else
            {
                var storageClientWrapper = this.ReusableClients.GetOrAdd(name, (key) => new StorageClientWrapper(name, factoryMethod.Invoke()));
                return (T)await storageClientWrapper.UnwrapAsync();
            }
        }

        public Task<IQueueClient> GetQueueClientAsync(string name) => this.GetOrCreateStorageClientAsync<IQueueClient>("queue", name);

        public Task<IBlobClient> GetBlobClientAsync(string name) => this.GetOrCreateStorageClientAsync<IBlobClient>("blob", name);

        public Task<IQueueClient> GetQueueClientAsync() => this.GetQueueClientAsync(Options.DefaultName);

        public Task<IBlobClient> GetBlobClientAsync() => this.GetBlobClientAsync(Options.DefaultName);

        public Task<ITableClient> GetTableClientAsync() => this.GetTableClientAsync(Options.DefaultName);

        public Task<ITableClient> GetTableClientAsync(string name) => this.GetOrCreateStorageClientAsync<ITableClient>("table", name);

        public void Dispose()
        {
            foreach (var entry in this.ReusableClients)
            {
                entry.Value.Dispose();
            }

            this.ReusableClients.Clear();
        }
    }
}