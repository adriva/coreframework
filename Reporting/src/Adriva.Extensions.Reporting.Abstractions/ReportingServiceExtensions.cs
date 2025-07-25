using System;
using Adriva.Extensions.Reporting.Abstractions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ReportingServiceExtensions
    {
        public static IServiceCollection AddReporting(this IServiceCollection services, Action<IReportingBuilder> build)
        {
            ReportingBuilder builder = new(services);
            services.AddSingleton<IReportingService, ReportingService>();
            builder.UseCommandBuilder<DefaultCommandBuilder>();
            builder.UseFilterValueBinder<DefaultFilterValueBinder>();
            builder.UseDataSource<EnumDataSource>(Constants.EnumDataSourceName);
            builder.UseDataSource<ObjectDataSource>(Constants.ObjectDataSourceName);
            build?.Invoke(builder);
            return services;
        }
    }
}