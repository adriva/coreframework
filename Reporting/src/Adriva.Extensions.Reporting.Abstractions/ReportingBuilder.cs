namespace Microsoft.Extensions.DependencyInjection
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