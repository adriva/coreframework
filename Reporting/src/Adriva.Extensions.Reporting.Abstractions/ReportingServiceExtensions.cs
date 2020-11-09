using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ReportingServiceExtensions
    {
        public static IServiceCollection AddReporting(this IServiceCollection services, Action<IReportingBuilder> build)
        {
            ReportingBuilder builder = new ReportingBuilder(services);
            build?.Invoke(builder);
            return services;
        }
    }
}