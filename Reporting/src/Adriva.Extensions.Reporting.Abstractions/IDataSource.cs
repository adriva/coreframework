using System.Collections.Generic;
using System.Threading.Tasks;

namespace Adriva.Extensions.Reporting.Abstractions
{
    public interface IDataSource
    {
        Task OpenAsync(DataSourceDefinition dataSourceDefinition);

        Task<DataSet> GetDataAsync(ReportCommand command, IEnumerable<FieldDefinition> fields);

        Task CloseAsync();
    }
}