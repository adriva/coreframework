using System;
using System.Threading.Tasks;

namespace Adriva.Extensions.Reporting.Abstractions
{
    public interface IDataSource
    {
        Task OpenAsync(DataSourceDefinition dataSourceDefinition);

        Task CloseAsync();
    }

    public sealed class DataSourceRegistrationOptions
    {
        public Type Type { get; private set; }

        public void UseType(Type dataSourceType)
        {
            this.Type = dataSourceType;
        }
    }
}