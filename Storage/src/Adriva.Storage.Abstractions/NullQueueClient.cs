using System;
using System.Threading;
using System.Threading.Tasks;

namespace Adriva.Storage.Abstractions
{
    public sealed class NullQueueClient : IQueueClient
    {
        public ValueTask InitializeAsync()
        {
            return new ValueTask();
        }

        public ValueTask AddAsync(QueueMessage message, TimeSpan? timeToLive, TimeSpan? initialVisibilityDelay)
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

        public ValueTask DisposeAsync()
        {
            return new ValueTask();
        }
    }
}