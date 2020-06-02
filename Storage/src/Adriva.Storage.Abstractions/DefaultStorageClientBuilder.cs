using System;
using Microsoft.Extensions.DependencyInjection;

namespace Adriva.Storage.Abstractions
{
    internal sealed class DefaultStorageClientBuilder : IStorageClientBuilder
    {
        public string Name { get; private set; }

        public IServiceCollection Services { get; }

        public DefaultStorageClientBuilder(string name, IServiceCollection services)
        {
            this.Name = name;
            this.Services = services;
        }

        public IStorageClientBuilder Configure<TOptions>(Action<TOptions> configure) where TOptions : class, new()
        {
            this.Services.Configure(this.Name, configure);
            return this;
        }
    }
}