using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace Adriva.Documents.Abstractions
{
    internal sealed class DefaultDocumentFactoryProvider : IDocumentFactoryProvider
    {
        private readonly IServiceProvider ServiceProvider;

        public DefaultDocumentFactoryProvider(IServiceProvider serviceProvider)
        {
            this.ServiceProvider = serviceProvider;
        }

        public TFactory GetFactory<TFactory>() where TFactory : IDocumentFactory
        {
            var targetFactory = this.ServiceProvider.GetServices<IDocumentFactory>().OfType<TFactory>().FirstOrDefault();

            if (null == targetFactory)
            {
                throw new InvalidOperationException($"Document factory {typeof(TFactory).FullName} could not be found.");
            }

            return targetFactory;
        }

        public IDocumentFactory<TDocument> GetFactoryForDocument<TDocument>() where TDocument : IDocument
        {
            var targetFactory = this.ServiceProvider.GetServices<IDocumentFactory>().OfType<IDocumentFactory<TDocument>>().FirstOrDefault();

            if (null == targetFactory)
            {
                throw new InvalidOperationException($"Document factory for document type {typeof(TDocument).FullName} could not be found.");
            }

            return targetFactory;
        }
    }
}