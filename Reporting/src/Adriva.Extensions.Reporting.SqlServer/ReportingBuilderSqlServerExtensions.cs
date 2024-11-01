using Adriva.Extensions.Reporting.Abstractions;
using Adriva.Extensions.Reporting.SqlServer;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ReportingBuilderSqlServerExtensions
    {
        public static IReportingBuilder UseSqlServer(this IReportingBuilder builder)
        {
            return builder.UseDataSource<SqlServerDataSource>("SqlServer");
        }
    }
}
