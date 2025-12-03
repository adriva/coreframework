using Microsoft.Extensions.DependencyInjection;

namespace Adriva.Extensions.Reporting.Abstractions
{
    public interface IReportingBuilder
    {
        IServiceCollection Services { get; }
    }
}