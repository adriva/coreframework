using System;
using System.Text;
using Adriva.Common.Core;

namespace Adriva.Storage.Abstractions
{
    public class DefaultQueueMessageSerializer : IQueueMessageSerializer
    {
        public QueueMessage Deserialize(byte[] content)
        {
            if (null == content || 0 == content.Length) return null;

            string json = Encoding.UTF8.GetString(content);
            return Utilities.SafeDeserialize<QueueMessage>(json);
        }

        public byte[] Serialize(QueueMessage queueMessage)
        {
            if (null == queueMessage) throw new ArgumentNullException(nameof(queueMessage));

            string json = Utilities.SafeSerialize(queueMessage);
            return Encoding.UTF8.GetBytes(json);
        }
    }
}
