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

        public ValueTask AddAsync(QueueMessage message, int ttlMinutes, int visibilityDelaySeconds)
        {
            return new ValueTask();
        }

        public Task<QueueMessage> GetNextAsync(int timeout, CancellationToken cancellationToken)
        {
            return Task.FromResult((QueueMessage)null);
        }

        public ValueTask DisposeAsync()
        {
            return new ValueTask();
        }
    }
}