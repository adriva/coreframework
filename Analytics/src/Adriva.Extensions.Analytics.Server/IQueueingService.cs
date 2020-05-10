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
        /// Returns an enumerable that will be used by the processors to get items from the queue and process and persist.
        /// <remarks>The IEnumerable instance returned must be thread safe and allow multiple readers and writers since it will be read by multiple processors (read) and handlers (write) at the same time.</remarks>
        /// </summary>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns>A instance of a class implementing IEnumerable&lt;AnalyticsItem&gt; interface.</returns>
        IEnumerable<AnalyticsItem> GetConsumingEnumerable(CancellationToken cancellationToken);
    }
}
