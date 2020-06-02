using System;

namespace Adriva.Storage.Abstractions
{
    public sealed class StorageClientFactoryOptions
    {
        internal Type ManagerType { get; private set; }

        internal bool IsSingleton { get; private set; }

        internal void AddStorageClient<T>(bool isSingleton = false) where T : class, IStorageClient
        {
            this.ManagerType = typeof(T);
            this.IsSingleton = isSingleton;
        }
    }
}