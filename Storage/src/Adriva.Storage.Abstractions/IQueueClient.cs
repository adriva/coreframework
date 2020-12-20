using System;
using System.Threading;
using System.Threading.Tasks;

namespace Adriva.Storage.Abstractions
{
    public interface IQueueClient : IStorageClient
    {
        ValueTask AddAsync(QueueMessage message, TimeSpan? visibilityTimeout = null, TimeSpan? initialVisibilityDelay = null);

        Task<QueueMessage> GetNextAsync(CancellationToken cancellationToken);

        Task DeleteAsync(QueueMessage message);
    }
}