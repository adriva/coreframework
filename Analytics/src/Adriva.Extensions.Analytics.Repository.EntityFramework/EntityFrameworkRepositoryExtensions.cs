using System;
using Adriva.Extensions.Analytics.Repository.EntityFramework;
using Adriva.Extensions.Analytics.Server;
using Microsoft.EntityFrameworkCore;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Provides extension methods to use Microsoft Entity Framework base repositories with the analytics server.
    /// </summary>
    public static class EntityFrameworkRepositoryExtensions
    {
        private static IAnalyticsServerBuilder UseEFRepository<TContext, TRepository>(this IAnalyticsServerBuilder builder, Action<DbContextOptionsBuilder> configureContext, ServiceLifetime contextLifetime = ServiceLifetime.Scoped, ServiceLifetime optionsLifetime = ServiceLifetime.Scoped)
                         where TContext : DbContext
                         where TRepository : class, IAnalyticsRepository
        {
            builder.Services.AddDbContext<TContext>(configureContext, contextLifetime, optionsLifetime);
            builder.UseRepository<TRepository>();
            return builder;
        }

        /// <summary>
        /// Registers an in-memory entity framework database to be used with the analytics server.
        /// <remarks>In-memory database should only be used for development / testing purposes and not in a production environment.</remarks>
        /// </summary>
        /// <param name="build">Analytics server builder delegate.</param>
        /// <param name="databaseName">The name of the in-memory database.</param>
        /// <returns>The Adriva.Extensions.Analytics.Server.IAnalyticsServerBuilder so that additional calls can be chained.</returns>
        public static IAnalyticsServerBuilder UseInMemoryRepository(this IAnalyticsServerBuilder build, string databaseName = "AnalyticsDb")
        {
            return build.UseEFRepository<AnalyticsDatabaseContext, InMemoryRepository>(dbContextBuilder => dbContextBuilder.UseInMemoryDatabase(databaseName), ServiceLifetime.Singleton, ServiceLifetime.Singleton);
        }
    }
}
