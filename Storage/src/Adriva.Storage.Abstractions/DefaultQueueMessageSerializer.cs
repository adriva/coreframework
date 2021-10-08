using System;
using Adriva.Common.Core;
using Newtonsoft.Json;

namespace Adriva.Storage.Abstractions
{
    public class DefaultQueueMessageSerializer : IQueueMessageSerializer
    {
        private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.All,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            Formatting = Formatting.None
        };

        public virtual JsonSerializerSettings GetSerializerSettings() => DefaultQueueMessageSerializer.SerializerSettings;

        public QueueMessage Deserialize(string content)
        {
            if (null == content || 0 == content.Length) return null;

            return Utilities.SafeDeserialize<QueueMessage>(content, this.GetSerializerSettings());
        }

        public string Serialize(QueueMessage queueMessage)
        {
            if (null == queueMessage) throw new ArgumentNullException(nameof(queueMessage));

            return Utilities.SafeSerialize(queueMessage, this.GetSerializerSettings());
        }
    }
}
