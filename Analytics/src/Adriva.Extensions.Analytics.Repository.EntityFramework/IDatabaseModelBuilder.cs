using Microsoft.EntityFrameworkCore;

namespace Adriva.Extensions.Analytics.Repository.EntityFramework
{
    /// <summary>
    /// Provides methods to configure the model used by the AnalyticsDbContext for different database providers.
    /// </summary>
    public interface IDatabaseModelBuilder
    {
        /// <summary>
        /// Called by the system once, to let the repository configure its model according to the database provider specifications.
        /// </summary>
        /// <param name="databaseContext">An instance of AnalyticsDatabaseContext that  will be used to persist analytics data.</param>
        /// <param name="modelBuilder">The ModelBuilder that can be used to configure the EF model.</param>
        void OnModelCreating(AnalyticsDatabaseContext databaseContext, ModelBuilder modelBuilder);
    }
}
