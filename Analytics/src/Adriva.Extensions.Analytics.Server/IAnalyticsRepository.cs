using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Adriva.Extensions.Analytics.Server.Entities;

namespace Adriva.Extensions.Analytics.Server
{
    /// <summary>
    /// Provides methods to persist analytics data in a repository.
    /// </summary>
    public interface IAnalyticsRepository
    {
        /// <summary>
        /// Called by the system once to let the repository do some initial work before accepting data.
        /// </summary>
        /// <returns>Represents the asynchronous process operation.</returns>
        Task InitializeAsync();

        /// <summary>
        /// Called by the system when the server analytics buffer is full to persist items in the repository.
        /// </summary>
        /// <param name="items">Items that should be persisted in the repository.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns>Represents the asynchronous process operation.</returns>
        Task StoreAsync(IEnumerable<AnalyticsItem> items, CancellationToken cancellationToken);

        /// <summary>
        /// Called by the system when the StoreAsync method encounters an exception.
        /// <remarks>This method should never throw any exceptions. Even if it does, it will be swallowed and ignored by the system.</remarks>
        /// </summary>
        /// <param name="items">Items that failed to be stored in the repository.</param>
        /// <param name="exception">The exception that is caught by the system.</param>
        /// <returns>Represents the asynchronous process operation.</returns>
        Task HandleErrorAsync(IEnumerable<AnalyticsItem> items, Exception exception);
    }
}
