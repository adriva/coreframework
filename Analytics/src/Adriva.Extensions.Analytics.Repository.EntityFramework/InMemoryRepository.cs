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
    /// Represents an analytics repository that stores data in-memory.
    /// </summary>
    public class InMemoryRepository : IAnalyticsRepository
    {
        private readonly SemaphoreSlim Semaphore = new SemaphoreSlim(1, 1);
        private readonly ILogger Logger;
        private readonly AnalyticsDatabaseContext Context;

        /// <summary>
        /// Initializes a new instance of Adriva.Extensions.Analytics.Repository.EntityFramework.InMemoryRepository class.
        /// </summary>
        /// <param name="analyticsDatabaseContext">The AnalyticsDatabaseContext object that will be used to store data.</param>
        /// <param name="logger">An instance of an ILogger object that is used to write log messages to.</param>
        public InMemoryRepository(AnalyticsDatabaseContext analyticsDatabaseContext, ILogger<InMemoryRepository> logger)
        {
            this.Context = analyticsDatabaseContext;
            this.Logger = logger;
        }

        /// <summary>
        /// Called by the system when the server analytics buffer is full to persist items in the repository.
        /// </summary>
        /// <param name="items">Items that should be persisted in the repository.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns>Represents the asynchronous process operation.</returns>
        public async virtual Task StoreAsync(IEnumerable<AnalyticsItem> items, CancellationToken cancellationToken)
        {
            this.Logger.LogTrace($"Persisting {items.Count()} analytics items in memory.");
            await this.Semaphore.WaitAsync();
            try
            {
                await this.Context.AddRangeAsync(items);
                await this.Context.SaveChangesAsync(cancellationToken);
            }
            finally
            {
                this.Semaphore.Release();
            }
            this.Logger.LogInformation($"Persisted {items.Count()} analytics items in memory.");
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
            this.Logger.LogError(exception, "InMemory repository encountered an exception when persisting analytics data.");
            return Task.CompletedTask;
        }
    }
}
