using System;
using Adriva.Documents.Abstractions;

namespace Microsoft.Extensions.DependencyInjection
{
    internal sealed class DefaultDocumentFactoryConfiguration : IDocumentFactoryConfiguration
    {
        private readonly IServiceCollection Services;

        public DefaultDocumentFactoryConfiguration(IServiceCollection services)
        {
            this.Services = services;
        }

        public IDocumentFactoryConfiguration AddFactory<TFactory>() where TFactory : class, IDocumentFactory
        {
            this.Services.AddSingleton<IDocumentFactory, TFactory>();
            return this;
        }

        public IDocumentFactoryConfiguration AddFactory<TFactory, TOptions>(Action<TOptions> configure)
             where TFactory : class, IDocumentFactory
             where TOptions : class, new()
        {
            this.Services.AddSingleton<IDocumentFactory, TFactory>();
            this.Services.Configure(configure);
            return this;
        }
    }
}