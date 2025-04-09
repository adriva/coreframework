using System;
using System.IO;
using System.Threading.Tasks;

namespace Adriva.Extensions.Reporting.Abstractions
{
    public interface IReportingService
    {
        Task<ReportDefinition> LoadReportDefinitionAsync(string name);

        Task PopulateFilterValuesAsync(ReportDefinition reportDefinition, FilterValuesDictionary values);

        Task<DataSet> GetFilterDataAsync(ReportDefinition reportDefinition, string filterName, FilterValuesDictionary values);

        Task<ReportOutput> ExecuteReportOutputAsync(string name, FilterValuesDictionary values);

        ValueTask RenderAsync<TRenderer>(string name, FilterValuesDictionary values, Stream stream, Func<TRenderer, ReportDefinition, ReportOutput, Task> interceptor = null) where TRenderer : ReportRenderer;

        Task<ReportOutput> ExecuteCommandAsync(string name, string commandName, FilterValuesDictionary values, string dataSourceName = null);
    }
}