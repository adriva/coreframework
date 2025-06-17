using System;
using Adriva.Documents.Abstractions;

namespace Microsoft.Extensions.DependencyInjection
{
    public interface IDocumentFactoryConfiguration
    {
        IDocumentFactoryConfiguration AddFactory<TFactory>() where TFactory : class, IDocumentFactory;

        IDocumentFactoryConfiguration AddFactory<TFactory, TOptions>(Action<TOptions> configure)
             where TFactory : class, IDocumentFactory
             where TOptions : class, new();
    }
}