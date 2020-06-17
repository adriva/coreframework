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

        private IStorageClientBuilder AddStorageClient<T>(string prefix, string name, ServiceLifetime serviceLifetime = ServiceLifetime.Singleton) where T : class, IStorageClient
        {
            if (null == name) throw new ArgumentNullException(nameof(name));

            name = string.Concat(prefix, ":", name);

            IStorageClientBuilder storageClientBuilder = new DefaultStorageClientBuilder(name, this.Services);

            this.Services.Configure<StorageClientFactoryOptions>(name, options =>
            {
                options.AddStorageClient<T>(serviceLifetime);
            });

            return storageClientBuilder;
        }

        public IStorageClientBuilder AddQueueClient<T>(ServiceLifetime serviceLifetime = ServiceLifetime.Singleton) where T : class, IQueueClient
        {
            return this.AddQueueClient<T>(Options.DefaultName, serviceLifetime);
        }

        public IStorageClientBuilder AddQueueClient<T>(string name, ServiceLifetime serviceLifetime = ServiceLifetime.Singleton) where T : class, IQueueClient
        {
            return this.AddStorageClient<T>("queue", name, serviceLifetime);
        }

        public IStorageClientBuilder AddBlobClient<T>(ServiceLifetime serviceLifetime = ServiceLifetime.Singleton) where T : class, IBlobClient
        {
            return this.AddBlobClient<T>(Options.DefaultName, serviceLifetime);
        }

        public IStorageClientBuilder AddBlobClient<T>(string name, ServiceLifetime serviceLifetime = ServiceLifetime.Singleton) where T : class, IBlobClient
        {
            return this.AddStorageClient<T>("blob", name, serviceLifetime);
        }

        public IStorageClientBuilder AddTableClient<T>(ServiceLifetime serviceLifetime = ServiceLifetime.Singleton) where T : class, ITableClient
        {
            return this.AddTableClient<T>(Options.DefaultName, serviceLifetime);
        }

        public IStorageClientBuilder AddTableClient<T>(string name, ServiceLifetime serviceLifetime = ServiceLifetime.Singleton) where T : class, ITableClient
        {
            return this.AddStorageClient<T>("table", name, serviceLifetime);
        }
    }
}