using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Adriva.Storage.Abstractions
{
    internal class DefaultStorageBuilder : IStorageBuilder
    {
        public IServiceCollection Services { get; private set; }

        public DefaultStorageBuilder(IServiceCollection services)
        {
            this.Services = services;
        }

        private IStorageClientBuilder AddStorageClient<T>(string prefix, string name, bool isSingleton = false) where T : class, IStorageClient
        {
            if (null == name) throw new ArgumentNullException(nameof(name));

            name = string.Concat(prefix, ":", name);

            IStorageClientBuilder storageClientBuilder = new DefaultStorageClientBuilder(name, this.Services);

            this.Services.Configure<StorageClientFactoryOptions>(name, options =>
            {
                options.AddStorageClient<T>(isSingleton);
            });

            return storageClientBuilder;
        }

        public IStorageClientBuilder AddQueueClient<T>(bool isSingleton = false) where T : class, IQueueClient
        {
            return this.AddQueueClient<T>(Options.DefaultName, isSingleton);
        }

        public IStorageClientBuilder AddQueueClient<T>(string name, bool isSingleton = false) where T : class, IQueueClient
        {
            return this.AddStorageClient<T>("queue", name, isSingleton);
        }

        public IStorageClientBuilder AddBlobClient<T>(bool isSingleton = false) where T : class, IBlobClient
        {
            return this.AddBlobClient<T>(Options.DefaultName, isSingleton);
        }

        public IStorageClientBuilder AddBlobClient<T>(string name, bool isSingleton = false) where T : class, IBlobClient
        {
            return this.AddStorageClient<T>("blob", name, isSingleton);
        }
    }
}