using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Adriva.Common.Core
{
    /// <summary>
    /// The base class for processor classes that can process QueueMessages.
    /// </summary>
    /// <typeparam name="TData">Type of the data encapsulated in the queue message.</typeparam>
    public abstract class AsyncQueueProcessor<TData> : IAsyncQueueProcessor
    {
        /// <summary>
        /// Processes the data that's encapsulated in a QueueMessage.
        /// </summary>
        /// <param name="data">The data object that represents the request to be processed.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <param name="logger">Represents a type used to perform logging.</param>
        /// <returns>Represents the asynchronous process operation.</returns>
        protected abstract Task ProcessAsync(TData data, CancellationToken cancellationToken, ILogger logger);

        /// <summary>
        /// Processes the request that is sent in a QueueMessage.
        /// </summary>
        /// <param name="message">The QueueMessage instance that encapsulates the request data and type.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <param name="logger">Represents a type used to perform logging.</param>
        /// <returns></returns>
        public async Task ProcessAsync(QueueMessage message, CancellationToken cancellationToken, ILogger logger)
        {
            if (null == message) return;

            TData data = Utilities.CastObject<TData>(message.Data);
            await this.ProcessAsync(data, cancellationToken, logger);
        }
    }
}
