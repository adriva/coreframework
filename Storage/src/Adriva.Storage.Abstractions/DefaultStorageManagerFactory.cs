using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace Adriva.Storage.Abstractions
{
    internal sealed class DefaultStorageManagerFactory : IStorageManagerFactory
    {
        private readonly IOptionsMonitor<StorageManagerFactoryOptions> OptionsMonitor;

        public DefaultStorageManagerFactory(IOptionsMonitor<StorageManagerFactoryOptions> optionsMonitor)
        {
            this.OptionsMonitor = optionsMonitor;

        }

        public Task<IQueueManager> GetQueueManagerAsync() => this.GetQueueManagerAsync("Default");

        public Task<IQueueManager> GetQueueManagerAsync(string name)
        {
            StorageManagerFactoryOptions storageManagerFactoryOptions = this.OptionsMonitor.Get(name);
            if (null == storageManagerFactoryOptions.ManagerType)
            {
                throw new KeyNotFoundException();
            }
            return null;
        }


    }
}