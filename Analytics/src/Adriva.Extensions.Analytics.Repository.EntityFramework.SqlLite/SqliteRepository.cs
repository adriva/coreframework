using System.Threading.Tasks;
using Adriva.Extensions.Analytics.Repository.EntityFramework;
using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Provides a default implementation of IAnalyticsRepository that uses Sqlite as the base data store.
    /// </summary>
    public class SqliteRepository : EntityFrameworkRepository
    {
        /// <summary>
        /// Initiates a new instance of SqliteRepository class.
        /// </summary>
        /// <param name="context">An instance of AnalyticsDatabaseContext that will be used to persist analytics data.</param>
        /// <param name="logger">An implementation of ILogger that will be used to write out log data.</param>
        public SqliteRepository(AnalyticsDatabaseContext context, ILogger<SqliteRepository> logger)
            : base(context, logger)
        {
        }

        /// <summary>
        /// Initializes the current instance of Adriva.Extensions.Analytics.Repository.EntityFramework.SqlLite.SqlLiteRepository class.
        /// </summary>
        /// <returns>Represents the asynchronous process operation.</returns>
        public override async Task InitializeAsync()
        {
            this.Logger.LogInformation("Ensuring database exists.");
            if (await this.Context.Database.EnsureCreatedAsync())
            {
                this.Logger.LogInformation("Analytics database is created.");
            }
            else
            {
                this.Logger.LogInformation("Analytics database already exists.");
            }
        }
    }
}
