using System;
using Adriva.Documents.Abstractions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDocuments(this IServiceCollection services, Action<IDocumentFactoryConfiguration> configure)
        {
            services.AddSingleton<IDocumentFactoryProvider, DefaultDocumentFactoryProvider>();

            DefaultDocumentFactoryConfiguration defaultDocumentFactoryConfiguration = new DefaultDocumentFactoryConfiguration(services);
            configure(defaultDocumentFactoryConfiguration);
            return services;
        }
    }
}