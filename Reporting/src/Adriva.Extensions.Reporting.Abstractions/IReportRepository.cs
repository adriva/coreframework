using System.Threading.Tasks;

namespace Adriva.Extensions.Reporting.Abstractions
{
    public interface IReportRepository
    {
        Task<ReportDefinition> LoadReportDefinitionAsync(string name);

        Task<RepositoryFile> GetRepositoryFileAsync(string name, bool isReportDefinition);
    }
}