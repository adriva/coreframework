using System;
using Microsoft.Extensions.DependencyInjection;

namespace Adriva.Storage.Abstractions
{
    public sealed class StorageClientFactoryOptions
    {
        internal Type ManagerType { get; private set; }

        internal ServiceLifetime ServiceLifetime { get; private set; }

        internal void AddStorageClient<T>(ServiceLifetime serviceLifetime) where T : class, IStorageClient
        {
            this.ManagerType = typeof(T);
            this.ServiceLifetime = serviceLifetime;
        }
    }
}