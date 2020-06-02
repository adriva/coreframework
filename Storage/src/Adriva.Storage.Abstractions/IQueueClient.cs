using System.Threading;
using System.Threading.Tasks;

namespace Adriva.Storage.Abstractions
{
    public interface IQueueClient : IStorageClient
    {
        ValueTask AddAsync(QueueMessage message, int ttlMinutes, int visibilityDelaySeconds);

        Task<QueueMessage> GetNextAsync(int timeout, CancellationToken cancellationToken);
    }
}