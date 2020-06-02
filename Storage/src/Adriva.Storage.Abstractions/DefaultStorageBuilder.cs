using System;
using Microsoft.Extensions.DependencyInjection;

namespace Adriva.Storage.Abstractions
{
    internal class DefaultStorageBuilder : IStorageBuilder
    {
        private readonly IServiceCollection Services;

        public DefaultStorageBuilder(IServiceCollection services)
        {
            this.Services = services;
        }

        private IStorageBuilder AddStorageClient<T>(string name, bool isSingleton = false) where T : class, IStorageClient
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));

            this.Services.Configure<StorageClientFactoryOptions>(name, options =>
            {
                options.AddStorageClient<T>(isSingleton);
            });
            return this;
        }

        public IStorageBuilder AddQueueClient<T>(bool isSingleton = false) where T : class, IQueueClient
        {
            return this.AddQueueClient<T>("Default", isSingleton);
        }

        public IStorageBuilder AddQueueClient<T>(string name, bool isSingleton = false) where T : class, IQueueClient
        {
            return this.AddStorageClient<T>(name, isSingleton);
        }
    }
}