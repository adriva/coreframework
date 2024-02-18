using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Adriva.Extensions.Reporting.Abstractions
{
    public interface IDataSource
    {
        Task OpenAsync(DataSourceDefinition dataSourceDefinition);

        Task<DataSet> GetDataAsync(ReportCommand command, FieldDefinition[] fields, JToken outputOptions);

        Task CloseAsync();
    }
}