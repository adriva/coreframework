using System;
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
        /// Registers the required services to use an EF Core based analytics repository.
        /// </summary>
        /// <param name="builder">Analytics server builder delegate.</param>
        /// <param name="configureContext">Callback method to configure DbContextOptions.</param>
        /// <param name="contextLifetime">The lifetime with which to register the DbContext service in the container.</param>
        /// <param name="optionsLifetime">The lifetime with which to register the DbContextOptions service in the container.</param>
        /// <typeparam name="TContext">The concrete implementation of a DbContext to use.</typeparam>
        /// <typeparam name="TRepository">The concrete implementation of an IAnalyticsRepository to use.</typeparam>
        /// <typeparam name="TModelBuilder">The concrete implementation of an IDatabaseModelBuilder to configure the model of the DbContext.</typeparam>
        /// <returns>The Adriva.Extensions.Analytics.Server.IAnalyticsServerBuilder so that additional calls can be chained.</returns>
        public static IAnalyticsServerBuilder UseEntityFrameworkRepository<TContext, TRepository, TModelBuilder>(this IAnalyticsServerBuilder builder,
                                Action<DbContextOptionsBuilder> configureContext,
                                ServiceLifetime contextLifetime = ServiceLifetime.Scoped,
                                ServiceLifetime optionsLifetime = ServiceLifetime.Scoped)
                         where TContext : DbContext
                         where TModelBuilder : class, IDatabaseModelBuilder
                         where TRepository : class, IAnalyticsRepository
        {
            builder.Services.AddDbContext<TContext>(configureContext, contextLifetime, optionsLifetime);
            builder.UseRepository<TRepository>();
            builder.Services.AddSingleton<IDatabaseModelBuilder, TModelBuilder>();
            return builder;
        }

        /// <summary>
        /// Registers the required services to use an EF Core based analytics repository.
        /// </summary>
        /// <param name="builder">Analytics server builder delegate.</param>
        /// <param name="configureContext">Callback method to configure DbContextOptions.</param>
        /// <param name="contextLifetime">The lifetime with which to register the DbContext service in the container.</param>
        /// <param name="optionsLifetime">The lifetime with which to register the DbContextOptions service in the container.</param>
        /// <typeparam name="TContext">The concrete implementation of a DbContext to use.</typeparam>
        /// <typeparam name="TRepository">The concrete implementation of an IAnalyticsRepository to use.</typeparam>
        /// <returns>The Adriva.Extensions.Analytics.Server.IAnalyticsServerBuilder so that additional calls can be chained.</returns>
        public static IAnalyticsServerBuilder UseEntityFrameworkRepository<TContext, TRepository>(this IAnalyticsServerBuilder builder,
                                        Action<DbContextOptionsBuilder> configureContext,
                                        ServiceLifetime contextLifetime = ServiceLifetime.Scoped,
                                        ServiceLifetime optionsLifetime = ServiceLifetime.Scoped)
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
        /// <param name="builder">Analytics server builder delegate.</param>
        /// <param name="databaseName">The name of the in-memory database.</param>
        /// <returns>The Adriva.Extensions.Analytics.Server.IAnalyticsServerBuilder so that additional calls can be chained.</returns>
        public static IAnalyticsServerBuilder UseInMemoryRepository(this IAnalyticsServerBuilder builder, string databaseName = "AnalyticsDb")
        {
            return builder.UseEntityFrameworkRepository<AnalyticsDatabaseContext, InMemoryRepository>(dbContextBuilder => dbContextBuilder.UseInMemoryDatabase(databaseName), ServiceLifetime.Singleton, ServiceLifetime.Singleton);
        }
    }
}
