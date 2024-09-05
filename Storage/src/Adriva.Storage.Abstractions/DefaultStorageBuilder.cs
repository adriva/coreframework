using System;
using System.Linq;
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

        private IStorageBuilder AddStorageClient<TClient, TOptions>(string qualifiedName, Action<TOptions> configure)
                                                                                where TClient : class, IStorageClient
                                                                                where TOptions : class, new()
        {
            this.Services.AddSingleton<IStorageClient, TClient>();
            this.Services.Configure(qualifiedName, configure);
            this.Services.PostConfigure<StorageClientRegistry>(registry =>
            {
                registry.RegisterWellKnownClient<TClient, TOptions>(qualifiedName);
            });
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

        public IStorageBuilder AddTableClient<TClient, TOptions>(string name, Action<TOptions> configure)
                where TClient : class, ITableClient
                where TOptions : class, new()
        {
            return this.AddStorageClient<TClient, TOptions>(Helpers.GetQualifiedTableName(name), configure);
        }
    }
}