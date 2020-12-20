using System;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;

namespace Adriva.Storage.Abstractions
{
    public sealed class StorageClientContext
    {
        private readonly string QualifiedName;

        public IServiceProvider ServiceProvider { get; private set; }

        public string Name { get; private set; }

        public TOptions GetOptions<TOptions>() where TOptions : class, new()
        {
            var optionsMonitor = this.ServiceProvider.GetRequiredService<IOptionsMonitor<TOptions>>();
            return optionsMonitor.Get(this.QualifiedName);
        }

        internal StorageClientContext(IServiceProvider serviceProvider, string qualifiedName, string name)
        {
            this.ServiceProvider = serviceProvider;
            this.QualifiedName = qualifiedName;
            this.Name = name;
        }
    }
}