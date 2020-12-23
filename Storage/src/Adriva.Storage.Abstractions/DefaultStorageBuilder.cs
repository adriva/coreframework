using System;
using System.Linq.Expressions;
using Adriva.Storage.Abstractions;

namespace Microsoft.Extensions.DependencyInjection
{
    internal class DefaultStorageBuilder : IStorageBuilder
    {
        public IServiceCollection Services { get; private set; }

        public DefaultStorageBuilder(IServiceCollection services)
        {
            this.Services = services;
        }

        private IStorageBuilder AddStorageClient<TClient, TOptions>(string name, Action<TOptions> configure)
                                                                                where TClient : class, IStorageClient
                                                                                where TOptions : class, new()
        {
            ServiceDescriptor serviceDescriptor = new ServiceDescriptor(typeof(StorageClientWrapper),
                (serviceProvider) =>
                {
                    Expression<Func<IStorageClient>> factoryMethod = () => ActivatorUtilities.CreateInstance<TClient>(serviceProvider);
                    return ActivatorUtilities.CreateInstance<StorageClientWrapper>(serviceProvider, factoryMethod, name);
                }, ServiceLifetime.Singleton);
            this.Services.Add(serviceDescriptor);
            this.Services.Configure(name, configure);
            return this;
        }

        public IStorageBuilder AddQueueClient<TClient, TOptions>(string name, Action<TOptions> configure)
                where TClient : class, IQueueClient
                where TOptions : class, new()
        {
            return this.AddStorageClient<TClient, TOptions>(Helpers.GetQualifiedQueueName(name), configure);
        }

        public IStorageBuilder AddBlobClient<TClient, TOptions>(string name, Action<TOptions> configure)
                where TClient : class, IBlobClient
                where TOptions : class, new()
        {
            return this.AddStorageClient<TClient, TOptions>(Helpers.GetQualifiedBlobName(name), configure);
        }
    }
}