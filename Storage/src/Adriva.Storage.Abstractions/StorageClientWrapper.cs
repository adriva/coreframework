using System.Threading;
using System.Threading.Tasks;

namespace Adriva.Storage.Abstractions
{
    internal class StorageClientWrapper : IStorageClient
    {
        private int IsInitialized = 0;
        internal IStorageClient StorageClient { get; private set; }

        public string Name { get; }

        public StorageClientWrapper(IStorageClient storageClient, string name)
        {
            this.StorageClient = storageClient;
            this.Name = name;
        }

        public async ValueTask InitializeAsync()
        {
            if (0 == Interlocked.CompareExchange(ref this.IsInitialized, 1, 0))
            {
                await this.StorageClient.InitializeAsync();
            }
        }

        public ValueTask DisposeAsync()
        {
            return this.StorageClient.DisposeAsync();
        }
    }
}