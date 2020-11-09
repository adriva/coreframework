using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.DependencyInjection
{
    public interface IReportingBuilder
    {
        IServiceCollection Services { get; }
    }
}