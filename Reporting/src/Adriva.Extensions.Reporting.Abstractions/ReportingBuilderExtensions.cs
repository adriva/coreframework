using System;
using Adriva.Extensions.Reporting.Abstractions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ReportingBuilderExtensions
    {
        public static IReportingBuilder UseCache(this IReportingBuilder builder, bool enabled = true, TimeSpan? timeToLive = null)
        {
            builder.Services.Configure<ReportingServiceOptions>(options =>
            {
                options.UseCache = enabled;
                options.DefinitionTimeToLive = timeToLive;
            });
            return builder;
        }

        public static IReportingBuilder UseFileSystemRepository(this IReportingBuilder builder, Action<FileSystemReportRepositoryOptions> configure)
        {
            builder.Services.AddSingleton<IReportRepository, FileSystemReportRepository>();
            builder.Services.Configure<FileSystemReportRepositoryOptions>(configure);
            return builder;
        }
    }
}