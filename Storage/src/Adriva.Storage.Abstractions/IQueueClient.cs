using System;
using System.Threading;
using System.Threading.Tasks;

namespace Adriva.Storage.Abstractions
{
    public interface IQueueClient : IStorageClient
    {
        ValueTask AddAsync(QueueMessage message, TimeSpan? timeToLive = null, TimeSpan? visibilityDelay = null);

        Task<QueueMessage> GetNextAsync(CancellationToken cancellationToken);
    }
}