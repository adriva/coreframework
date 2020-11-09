using System;
using Adriva.Extensions.Reporting.Abstractions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ReportingBuilderExtensions
    {
        public static IReportingBuilder UseFileSystemRepository(this IReportingBuilder builder, Action<FileSystemReportRepositoryOptions> configure)
        {
            builder.Services.AddSingleton<IReportRepository, FileSystemReportRepository>();
            builder.Services.Configure<FileSystemReportRepositoryOptions>(configure);
            return builder;
        }
    }
}