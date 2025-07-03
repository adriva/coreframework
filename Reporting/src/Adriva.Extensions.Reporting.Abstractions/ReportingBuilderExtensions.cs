using System;
using Adriva.Extensions.Reporting.Abstractions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ReportingBuilderExtensions
    {
        public static IReportingBuilder AllowSensitiveData(this IReportingBuilder builder, bool isAllowed = true)
        {
            builder.Services.Configure<ReportingServiceOptions>(options =>
            {
                options.AllowSensitiveData = isAllowed;
            });
            return builder;
        }

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

        public static IReportingBuilder UseDataSource<TDataSource>(this IReportingBuilder builder, string name) where TDataSource : class, IDataSource
        {
            builder.Services.AddScoped<TDataSource>();

            builder.Services.Configure<DataSourceRegistrationOptions>(name, options =>
            {
                options.UseType(typeof(TDataSource).TypeHandle);
            });
            return builder;
        }

        public static IReportingBuilder UseDataSource<TDataSource, TOptions>(this IReportingBuilder builder, string name, Action<TOptions> configure)
                                                                                                where TDataSource : class, IDataSource
                                                                                                where TOptions : class
        {
            builder.UseDataSource<TDataSource>(name);
            builder.Services.Configure<TOptions>(configure);
            return builder;
        }

        public static IReportingBuilder UseCommandBuilder<TBuilder>(this IReportingBuilder builder) where TBuilder : class, ICommandBuilder
        {
            builder.Services.AddSingleton<ICommandBuilder, TBuilder>();
            return builder;
        }

        public static IReportingBuilder UseFilterValueBinder<TBinder>(this IReportingBuilder builder) where TBinder : class, IFilterValueBinder
        {
            builder.Services.AddSingleton<IFilterValueBinder, TBinder>();
            return builder;
        }

        public static IReportingBuilder AddRenderer<TRenderer>(this IReportingBuilder builder) where TRenderer : ReportRenderer
        {
            builder.Services.AddScoped<ReportRenderer, TRenderer>();
            return builder;
        }
    }
}