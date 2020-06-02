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

        private IStorageBuilder AddManager<T>(string name) where T : class, IStorageManager
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));

            this.Services.Configure<StorageManagerFactoryOptions>(name, options => options.AddManager<T>());
            return this;
        }

        public IStorageBuilder AddQueueManager<T>(string name) where T : class, IQueueManager
        {
            return this.AddManager<T>(name);
        }
    }
}