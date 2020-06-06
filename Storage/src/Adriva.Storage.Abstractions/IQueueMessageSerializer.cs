using System.Threading.Tasks;

namespace Adriva.Storage.Abstractions
{
    public interface IQueueMessageSerializer
    {
        Task<byte[]> SerializeAsync(QueueMessage queueMessage);

        Task<QueueMessage> DeserializeAsync(byte[] content);
    }
}
