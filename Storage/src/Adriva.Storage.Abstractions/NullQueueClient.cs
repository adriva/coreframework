using System;
using System.Threading;
using System.Threading.Tasks;

namespace Adriva.Storage.Abstractions
{
    public sealed class NullQueueClient : IQueueClient
    {
        public ValueTask InitializeAsync(StorageClientContext context)
        {
            return new ValueTask();
        }

        public ValueTask AddAsync(QueueMessage message, TimeSpan? timetoLive, TimeSpan? timeToLive, TimeSpan? initialVisibilityDelay)
        {
            return new ValueTask();
        }

        public Task<QueueMessage> GetNextAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult((QueueMessage)null);
        }

        public Task DeleteAsync(QueueMessage message)
        {
            return Task.CompletedTask;
        }
    }
}