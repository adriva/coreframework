using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Adriva.Extensions.Analytics.Server.Entities;

namespace Adriva.Extensions.Analytics.Server
{
    /// <summary>
    /// Provides an empty implementation of an analytics repository that does not persist data or handle any exceptions.
    /// </summary>
    public sealed class NullRepository : IAnalyticsRepository
    {
        /// <summary>
        /// Called by the system when the server analytics buffer is full to persist items in the repository.
        /// </summary>
        /// <param name="items">Items that should be persisted in the repository.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns>Represents the asynchronous process operation.</returns>
        public Task HandleErrorAsync(IEnumerable<AnalyticsItem> items, Exception exception)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Called by the system when the StoreAsync method encounters an exception.
        /// <remarks>This method should never throw any exceptions. Even if it does, it will be swallowed and ignored by the system.</remarks>
        /// </summary>
        /// <param name="items">Items that failed to be stored in the repository.</param>
        /// <param name="exception">The exception that is caught by the system.</param>
        /// <returns>Represents the asynchronous process operation.</returns>
        public Task StoreAsync(IEnumerable<AnalyticsItem> items, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
