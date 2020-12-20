using System;
using Adriva.Storage.Abstractions;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class Helpers
    {
        public static string GetQueueName(string name)
        {
            return $"queue:{name}";
        }
    }

    public interface IStorageBuilder
    {
        IServiceCollection Services { get; }

        IStorageBuilder AddQueueClient<TClient, TOptions>(string name, ServiceLifetime serviceLifetime, Action<TOptions> configure)
                                                where TClient : class, IQueueClient
                                                where TOptions : class, new();
    }

    internal class DefaultStorageBuilder : IStorageBuilder
    {
        public IServiceCollection Services { get; private set; }

        public DefaultStorageBuilder(IServiceCollection services)
        {
            this.Services = services;
        }

        private IStorageBuilder AddStorageClient<TClient, TOptions>(string name, ServiceLifetime serviceLifetime, Action<TOptions> configure)
                                                                                where TClient : class, IStorageClient
                                                                                where TOptions : class, new()
        {
            ServiceDescriptor serviceDescriptor = new ServiceDescriptor(typeof(StorageClientWrapper),
                (serviceProvider) =>
                {
                    Func<IStorageClient> factoryMethod = () => ActivatorUtilities.CreateInstance<TClient>(serviceProvider);
                    return new StorageClientWrapper(factoryMethod, name);
                }, serviceLifetime);
            this.Services.Add(serviceDescriptor);
            this.Services.Configure(name, configure);
            return this;
        }

        public IStorageBuilder AddQueueClient<TClient, TOptions>(string name, ServiceLifetime serviceLifetime, Action<TOptions> configure)
                where TClient : class, IQueueClient
                where TOptions : class, new()
        {
            return this.AddStorageClient<TClient, TOptions>(Helpers.GetQueueName(name), serviceLifetime, configure);
        }
    }

    public static class StorageExtensions
    {
        public static IStorageBuilder AddStorage(this IServiceCollection services)
        {
            services.AddSingleton<IStorageClientFactory>(serviceProvider => ActivatorUtilities.CreateInstance<DefaultStorageClientFactory>(serviceProvider));
            DefaultStorageBuilder builder = new DefaultStorageBuilder(services);

            return builder;
        }
    }
}