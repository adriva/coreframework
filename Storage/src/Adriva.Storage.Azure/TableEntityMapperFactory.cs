using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;

namespace Adriva.Storage.Azure
{
    public sealed class TableEntityMapperFactory : ITableEntityMapperFactory
    {
        private readonly IServiceProvider ServiceProvider;
        private readonly ConcurrentDictionary<Type, object> MapperCache = new ConcurrentDictionary<Type, object>();

        public TableEntityMapperFactory(IServiceProvider serviceProvider)
        {
            this.ServiceProvider = serviceProvider;
        }

        public ITableEntityMapper<T> GetMapper<T>() where T : class
        {
            return (ITableEntityMapper<T>)this.MapperCache.GetOrAdd(typeof(T), _ => ActivatorUtilities.CreateInstance<TableEntityMapper<T>>(this.ServiceProvider));
        }
    }
}
