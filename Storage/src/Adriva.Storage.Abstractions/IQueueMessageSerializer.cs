namespace Adriva.Storage.Abstractions
{
    public interface IQueueMessageSerializer
    {
        string Serialize(QueueMessage queueMessage);

        QueueMessage Deserialize(string content);
    }
}
