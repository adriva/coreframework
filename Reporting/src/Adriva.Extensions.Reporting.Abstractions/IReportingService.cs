using System.Threading.Tasks;

namespace Adriva.Extensions.Reporting.Abstractions
{
    public interface IReportingService
    {
        Task<ReportDefinition> LoadReportDefinitionAsync(string name);

        Task PopulateFilterValuesAsync(ReportDefinition reportDefinition, FilterValuesDictionary values);

        Task<DataSet> GetFilterDataAsync(ReportDefinition reportDefinition, string filterName, FilterValuesDictionary values);

        Task<ReportOutput> ExecuteReportOutputAsync(ReportDefinition reportDefinition, FilterValuesDictionary values);
    }
}