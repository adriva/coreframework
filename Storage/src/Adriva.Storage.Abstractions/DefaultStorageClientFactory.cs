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
            private bool IsInitialized = false;
            private SemaphoreSlim InitializerSemaphore = new SemaphoreSlim(1, 1);

            public StorageClientWrapper(IStorageClient storageClient)
            {
                this.StorageClient = storageClient;
            }

            public async Task<IStorageClient> UnwrapAsync()
            {
                if (this.IsInitialized) return this.StorageClient;

                await this.InitializerSemaphore.WaitAsync();
                try
                {
                    if (!this.IsInitialized)
                    {
                        await this.StorageClient.InitializeAsync();
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
        private readonly IOptionsMonitor<StorageClientFactoryOptions> OptionsMonitor;
        private readonly ConcurrentDictionary<string, StorageClientWrapper> ReusableClients = new ConcurrentDictionary<string, StorageClientWrapper>();

        public DefaultStorageClientFactory(IServiceProvider serviceProvider, IOptionsMonitor<StorageClientFactoryOptions> optionsMonitor)
        {
            this.ServiceProvider = serviceProvider;
            this.OptionsMonitor = optionsMonitor;
        }

        private async Task<T> GetOrCreateStorageClientAsync<T>(string name) where T : IStorageClient
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));

            var options = this.OptionsMonitor.Get(name);

            if (null == options.ManagerType)
            {
                throw new InvalidOperationException($"No storage managers with the name '{name}' is registered.");
            }

            Func<T> factoryMethod = new Func<T>(() =>
            {
                return (T)ActivatorUtilities.CreateInstance(this.ServiceProvider, options.ManagerType);
            });

            if (!options.IsSingleton)
            {
                return factoryMethod.Invoke();
            }
            else
            {
                var storageClientWrapper = this.ReusableClients.GetOrAdd(name, (key) => new StorageClientWrapper(factoryMethod.Invoke()));
                return (T)await storageClientWrapper.UnwrapAsync();
            }
        }


        public Task<IQueueClient> GetQueueClientAsync() => this.GetQueueClientAsync("Default");

        public Task<IQueueClient> GetQueueClientAsync(string name) => this.GetOrCreateStorageClientAsync<IQueueClient>(name);

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