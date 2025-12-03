using System.Threading.Tasks;

namespace Adriva.Extensions.Reporting.Abstractions
{
    public interface IFilterValueBinder
    {
        Task<FilterValue> GetFilterValueAsync(ReportContext context, FilterDefinition filterDefinition, string rawValue);
    }
}