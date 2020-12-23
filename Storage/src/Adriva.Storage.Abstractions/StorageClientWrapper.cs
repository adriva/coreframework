using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Adriva.Storage.Abstractions
{
    internal class StorageClientWrapper : IStorageClient
    {
        private readonly Func<IStorageClient> ClientFactory;
        private int IsInitialized = 0;
        private IStorageClient StorageClientField = null;

        internal IStorageClient StorageClient
        {
            get
            {
                Interlocked.CompareExchange(ref this.StorageClientField, this.ClientFactory.Invoke(), null);
                return this.StorageClientField;
            }
        }

        public string Name { get; private set; }

        public StorageClientWrapper(Func<IStorageClient> clientFactory, string name)
        {
            if (null == name) throw new ArgumentNullException(nameof(name));

            this.ClientFactory = clientFactory;
            this.Name = name;
        }

        public async ValueTask InitializeAsync(StorageClientContext context)
        {
            if (0 == Interlocked.CompareExchange(ref this.IsInitialized, 1, 0))
            {
                await this.StorageClient.InitializeAsync(context);
            }
        }

        public ValueTask DisposeAsync()
        {
            return this.StorageClient.DisposeAsync();
        }

        public void Dispose()
        {
            this.StorageClient.Dispose();
        }
    }
}