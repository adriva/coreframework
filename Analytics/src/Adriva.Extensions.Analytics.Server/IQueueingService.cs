using System.Collections.Generic;
using System.Threading;
using Adriva.Extensions.Analytics.Server.Entities;

namespace Adriva.Extensions.Analytics.Server
{
    /// <summary>
    /// Provides methods and properties to queue analytics items in a server buffer and share it with processors to persist in an analytics repository.
    /// </summary>
    public interface IQueueingService
    {
        /// <summary>
        /// Returns a value indicating if adding to the queue is completed and no other items will be added.
        /// <remarks>When the applicaion about to shutdown, this property should return True to indicate the consumers that no more items will be added to the queue.</remarks>
        /// </summary>
        /// <value>A boolean value indicating if the adding to the queue is completed.</value>
        bool IsCompleted { get; }

        /// <summary>
        /// Adds an analytics item to the queue to be processed by the system.
        /// </summary>
        /// <param name="analyticsItem">An instance of AnalyticsItem class to be processed.</param>
        void Enqueue(AnalyticsItem analyticsItem);

        /// <summary>
        /// Tries to get the next item from the queue in a given time period.
        /// </summary>
        /// <param name="millisecondsTimeout">Maximum time to wait in milliseconds before timing out.</param>
        /// <param name="analyticsItem">The AnalyticsItem instance that is retrieved from the queue, if possible.</param>
        /// <returns>A boolean value indicating if the AnalyticsItem could be retrieved or not.</returns>
        bool TryGetNext(int millisecondsTimeout, out AnalyticsItem analyticsItem);
    }
}
