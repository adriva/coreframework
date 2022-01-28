using System.Collections.Generic;
using System.Threading.Tasks;

namespace Adriva.Extensions.Reporting.Abstractions
{
    public interface IReportingService
    {
        Task<ReportDefinition> LoadReportDefinitionAsync(string name);

        Task PopulateFilterValuesAsync(ReportDefinition reportDefinition, IDictionary<string, string> values);

        Task<ReportOutput> ExecuteReportOutputAsync(ReportDefinition reportDefinition, IDictionary<string, string> values);
    }
}