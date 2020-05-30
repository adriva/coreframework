using System;
using Adriva.Extensions.Analytics.Server;
using System.Threading.Tasks;
using System.Collections.Generic;
using Adriva.Extensions.Analytics.Server.Entities;
using System.Threading;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace Adriva.Extensions.Analytics.Repository.EntityFramework
{
    /// <summary>
    /// Represents a base class for an analytics repository that stores data using EF Core.
    /// </summary>
    public abstract class EntityFrameworkRepository : IAnalyticsRepository
    {
        /// <summary>
        /// Gets the logger that can be used to write log output.
        /// </summary>
        /// <value>A concrete implementation of ILogger interface.</value>
        protected ILogger Logger { get; private set; }

        /// <summary>
        /// Gets an instance of AnalyticsDatabaseContext class that is used to persist analytics data.
        /// </summary>
        /// <value>An instance of AnalyticsDatabaseContext class.</value>
        protected AnalyticsDatabaseContext Context { get; private set; }

        /// <summary>
        /// Initiates a new instance of Adriva.Extensions.Analytics.Repository.EntityFramework.EntityFrameworkRepository class.
        /// </summary>
        /// <param name="analyticsDatabaseContext">The AnalyticsDatabaseContext object that will be used to store data.</param>
        /// <param name="logger">An instance of an ILogger object that is used to write log messages to.</param>
        protected EntityFrameworkRepository(AnalyticsDatabaseContext analyticsDatabaseContext, ILogger logger)
        {
            this.Context = analyticsDatabaseContext;
            this.Logger = logger;
        }

        /// <summary>
        /// Initializes the current instance of Adriva.Extensions.Analytics.Repository.EntityFramework.EntityFrameworkRepository class.
        /// </summary>
        /// <returns>Represents the asynchronous process operation.</returns>
        public virtual Task InitializeAsync() => Task.CompletedTask;

        /// <summary>
        /// Called by the system when the server analytics buffer is full to persist items in the repository.
        /// </summary>
        /// <param name="items">Items that should be persisted in the repository.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns>Represents the asynchronous process operation.</returns>
        public async virtual Task StoreAsync(IEnumerable<AnalyticsItem> items, CancellationToken cancellationToken)
        {
            this.Logger.LogTrace($"Persisting {items.Count()} analytics items.");
            await this.Context.AddRangeAsync(items);
            await this.Context.SaveChangesAsync(cancellationToken);
            this.Logger.LogInformation($"Persisted {items.Count()} analytics items.");
        }

        /// <summary>
        /// Called by the system when the StoreAsync method encounters an exception.
        /// <remarks>In-memory repository only writes the error message to the log and returns.</remarks>
        /// </summary>
        /// <param name="items">Items that failed to be stored in the repository.</param>
        /// <param name="exception">The exception that is caught by the system.</param>
        /// <returns>Represents the asynchronous process operation.</returns>
        public virtual Task HandleErrorAsync(IEnumerable<AnalyticsItem> items, Exception exception)
        {
            this.Logger.LogError(exception, "Analytics data repository encountered an exception when persisting analytics data.");
            return Task.CompletedTask;
        }
    }
}
