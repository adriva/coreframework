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
    public class InMemoryRepository : EntityFrameworkRepository
    {
        private readonly SemaphoreSlim Semaphore = new SemaphoreSlim(1, 1);

        /// <summary>
        /// Initiates a new instance of Adriva.Extensions.Analytics.Repository.EntityFramework.InMemoryRepository class.
        /// </summary>
        /// <param name="analyticsDatabaseContext">The AnalyticsDatabaseContext object that will be used to store data.</param>
        /// <param name="logger">An instance of an ILogger object that is used to write log messages to.</param>
        public InMemoryRepository(AnalyticsDatabaseContext analyticsDatabaseContext, ILogger<InMemoryRepository> logger)
            : base(analyticsDatabaseContext, logger)
        {

        }

        /// <summary>
        /// Called by the system when the server analytics buffer is full to persist items in the repository.
        /// </summary>
        /// <param name="items">Items that should be persisted in the repository.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns>Represents the asynchronous process operation.</returns>
        public async override Task StoreAsync(IEnumerable<AnalyticsItem> items, CancellationToken cancellationToken)
        {
            this.Logger.LogTrace($"Persisting {items.Count()} analytics items in memory.");
            await this.Semaphore.WaitAsync();
            try
            {
                await base.StoreAsync(items, cancellationToken);
            }
            finally
            {
                this.Semaphore.Release();
            }
            this.Logger.LogInformation($"Persisted {items.Count()} analytics items in memory.");
        }
    }
}
