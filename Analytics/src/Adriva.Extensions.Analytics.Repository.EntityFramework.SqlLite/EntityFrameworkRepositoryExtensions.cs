using Adriva.Extensions.Analytics.Repository.EntityFramework;
using Adriva.Extensions.Analytics.Server;
using Microsoft.EntityFrameworkCore;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Provides extension methods to use Microsoft Entity Framework based repositories with the analytics server.
    /// </summary>
    public static class EntityFrameworkRepositoryExtensions
    {
        /// <summary>
        /// Registers a Sqlite entity framework database to be used with the analytics server.
        /// </summary>
        /// <param name="builder">Analytics server builder delegate.</param>
        /// <param name="connectionString">The connection string that will be used by the Sqlite provider.</param>
        /// <returns>The Adriva.Extensions.Analytics.Server.IAnalyticsServerBuilder so that additional calls can be chained.</returns>
        public static IAnalyticsServerBuilder UseSqlite(this IAnalyticsServerBuilder builder, string connectionString = "Data Source=./analytics.db")
        {
            return builder.UseEntityFrameworkRepository<AnalyticsDatabaseContext, SqliteRepository>(
                dbContextBuilder =>
                {
                    dbContextBuilder.UseSqlite(connectionString);
                },
                ServiceLifetime.Scoped,
                ServiceLifetime.Singleton);
        }
    }
}
