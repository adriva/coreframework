using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Adriva.Common.Core
{
    /// <summary>
    /// Represents a processor class that is capable of processing queue messages.
    /// </summary>
    public interface IAsyncQueueProcessor
    {
        /// <summary>
        /// Processes a queue message
        /// </summary>
        /// <param name="message">The message object that is to be processed.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <param name="logger">Represents a type used to perform logging.</param>
        /// <returns>A task that represents the async operation.</returns>
        Task ProcessAsync(QueueMessage message, CancellationToken cancellationToken, ILogger logger);
    }
}
