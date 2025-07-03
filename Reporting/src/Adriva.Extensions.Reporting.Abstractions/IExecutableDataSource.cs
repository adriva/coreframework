using System.Threading.Tasks;

namespace Adriva.Extensions.Reporting.Abstractions
{
    public interface IExecutableDataSource : IDataSource
    {
        Task<object> ExecuteAsync(ReportCommand command);
    }
}