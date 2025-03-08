using Microsoft.Extensions.DependencyInjection;

namespace Adriva.Extensions.Reporting.Abstractions
{
    internal class ReportingBuilder : IReportingBuilder
    {
        public IServiceCollection Services { get; private set; }

        public ReportingBuilder(IServiceCollection services)
        {
            this.Services = services;
        }
    }
}