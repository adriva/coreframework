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

        public static IReportingBuilder UseRepository<TRepository, TOptions>(this IReportingBuilder builder, Action<TOptions> configure)
                                                                                                where TRepository : class, IReportRepository
                                                                                                where TOptions : class
        {
            builder.Services.AddSingleton<IReportRepository, TRepository>();
            builder.Services.Configure<TOptions>(configure);
            return builder;
        }

        public static IReportingBuilder UseFileSystemRepository(this IReportingBuilder builder, Action<FileSystemReportRepositoryOptions> configure)
        {
            return builder.UseRepository<FileSystemReportRepository, FileSystemReportRepositoryOptions>(configure);
        }
    }
}