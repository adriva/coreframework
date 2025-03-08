using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Adriva.Storage.Abstractions;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection
{
    public sealed class StorageClientRegistry
    {
        private readonly ConcurrentDictionary<string, Func<IServiceProvider, Lazy<Task<IStorageClient>>>> ClientActivators;

        public StorageClientRegistry()
        {
            this.ClientActivators = new ConcurrentDictionary<string, Func<IServiceProvider, Lazy<Task<IStorageClient>>>>();
        }

        internal void RegisterWellKnownClient<TClient, TOptions>(string qualifiedName)
                                                                where TClient : IStorageClient
                                                                where TOptions : class, new()
        {
            this.ClientActivators.TryAdd(qualifiedName, serviceProvider => new Lazy<Task<IStorageClient>>(async () =>
            {
                IStorageClient storageClient = ActivatorUtilities.GetServiceOrCreateInstance<TClient>(serviceProvider);
                if (storageClient is IAsyncInitializedStorageClient<TOptions> asyncInitializedStorageClient)
                {
                    var optionsMonitor = serviceProvider.GetRequiredService<IOptionsMonitor<TOptions>>();
                    TOptions storageClientOptions = optionsMonitor.Get(qualifiedName);
                    await asyncInitializedStorageClient.InitializeAsync(storageClientOptions);
                }
                return storageClient;
            }));
        }

        public bool TryTakeClientActivator(string qualifiedName, out Func<IServiceProvider, Lazy<Task<IStorageClient>>> activator)
        {
            return this.ClientActivators.TryRemove(qualifiedName, out activator);
        }
    }
}