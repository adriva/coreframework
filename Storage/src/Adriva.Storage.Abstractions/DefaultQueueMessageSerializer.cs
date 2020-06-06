using System;
using System.Text;
using System.Threading.Tasks;
using Adriva.Common.Core;

namespace Adriva.Storage.Abstractions
{
    public class DefaultQueueMessageSerializer : IQueueMessageSerializer
    {
        public Task<QueueMessage> DeserializeAsync(byte[] content)
        {
            if (null == content || 0 == content.Length) return null;

            string json = Encoding.UTF8.GetString(content);
            return Task.FromResult(Utilities.SafeDeserialize<QueueMessage>(json));
        }

        public Task<byte[]> SerializeAsync(QueueMessage queueMessage)
        {
            if (null == queueMessage) throw new ArgumentNullException(nameof(queueMessage));

            string json = Utilities.SafeSerialize(queueMessage);
            return Task.FromResult(Encoding.UTF8.GetBytes(json));
        }
    }
}
