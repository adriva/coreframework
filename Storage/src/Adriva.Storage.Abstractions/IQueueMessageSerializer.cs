namespace Adriva.Storage.Abstractions
{
    public interface IQueueMessageSerializer
    {
        byte[] Serialize(QueueMessage queueMessage);

        QueueMessage Deserialize(byte[] content);
    }
}
