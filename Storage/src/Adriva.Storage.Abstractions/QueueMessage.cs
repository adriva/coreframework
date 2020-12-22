using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;

namespace Adriva.Storage.Abstractions
{
    /// <summary>
    /// Represents a message that can be used in a queue system.
    /// </summary>
    [Serializable]
    public sealed class QueueMessage
    {
        [JsonProperty("id", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Id { get; private set; }

        /// <summary>
        /// Gets the flags set on the message.
        /// </summary>
        /// <value>An instance of QueueMessageFlags class that represents the flags set on the message.</value>
        [JsonProperty("fl", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public QueueMessageFlags Flags { get; private set; } = QueueMessageFlags.NormalPriority;

        [JsonProperty("ct", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string CommandType { get; private set; }

        [JsonProperty("d", TypeNameHandling = TypeNameHandling.All, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public object Data { get; private set; }

        [JsonProperty("pt", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string PlatformTag { get; set; }

        [JsonConstructor]
        private QueueMessage() { }

        public static QueueMessage Create(object data, string commandType, QueueMessageFlags flags = null)
        {
            flags = flags ?? QueueMessageFlags.NormalPriority;
            return new QueueMessage()
            {
                CommandType = commandType,
                Data = data,
                Flags = flags
            };
        }

        public QueueMessage WithData(object data)
        {
            var clone = this.Clone();
            clone.Data = data;
            return clone;
        }

        public QueueMessage WithFlags(QueueMessageFlags flags)
        {
            var clone = this.Clone();
            clone.Flags = flags;
            return clone;
        }

        public QueueMessage SetId(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentNullException(nameof(id));
            if (!string.IsNullOrWhiteSpace(this.Id))
                throw new InvalidOperationException("Once queue message id is assigned it cannot be changed.");

            this.Id = id;
            return this;
        }

        public QueueMessage Clone()
        {
            object cloneData = null;

            if (this.Data is ICloneable cloneableData) cloneData = cloneableData.Clone();
            else cloneData = this.Data;

            QueueMessage message = new QueueMessage();
            message.Flags = (long)this.Flags;
            message.CommandType = this.CommandType;
            message.Id = this.Id;
            return message;
        }
    }
}
