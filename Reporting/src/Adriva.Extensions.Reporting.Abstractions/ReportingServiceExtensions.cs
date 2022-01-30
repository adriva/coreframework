using System;
using Adriva.Extensions.Reporting.Abstractions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ReportingServiceExtensions
    {
        public static IServiceCollection AddReporting(this IServiceCollection services, Action<IReportingBuilder> build)
        {
            ReportingBuilder builder = new ReportingBuilder(services);
            services.AddSingleton<IReportingService, ReportingService>();
            builder.UseCommandBuilder<DefaultCommandBuilder>();
            builder.UseFilterValueBinder<DefaultFilterValueBinder>();
            builder.UseDataSource<EnumDataSource>("Enum");
            builder.UseDataSource<ObjectDataSource>("Object");
            build?.Invoke(builder);
            return services;
        }
    }
}