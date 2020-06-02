using System;

namespace Adriva.Storage.Abstractions
{
    public sealed class StorageManagerFactoryOptions
    {
        internal Type ManagerType { get; private set; }

        internal void AddManager<T>() where T : class, IStorageManager
        {
            this.ManagerType = typeof(T);
        }
    }
}