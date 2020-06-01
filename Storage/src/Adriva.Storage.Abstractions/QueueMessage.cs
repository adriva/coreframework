using Newtonsoft.Json;
using System;

namespace Adriva.Storage.Abstractions
{
    /// <summary>
    /// Represents a message that can be used in a queue system.
    /// </summary>
    [Serializable]
    public class QueueMessage
    {
        /// <summary>
        /// Gets the flags set on the message.
        /// </summary>
        /// <value>An instance of QueueMessageFlags class that represents the flags set on the message.</value>
        [JsonProperty("fl")]
        public QueueMessageFlags Flags { get; private set; } = QueueMessageFlags.None;

        [JsonProperty("li")]
        public string LockIdentifier { get; private set; }

        [JsonProperty("ct")]
        public string CommandType { get; private set; }

        [JsonProperty("d")]
        public object Data { get; private set; }

        public QueueMessage(string commandType, object data)
            : this(QueueMessageFlags.None, commandType, data, null)
        {
        }

        public QueueMessage(string commandType, object data, string lockIdentifier)
            : this(QueueMessageFlags.None, commandType, data, lockIdentifier)
        {
        }

        [JsonConstructor]
        public QueueMessage(QueueMessageFlags flags, string commandType, object data, string lockIdentifier)
        {
            this.CommandType = commandType;
            this.Flags = flags;
            this.Data = data;
            this.LockIdentifier = lockIdentifier;

            if (string.IsNullOrWhiteSpace(this.LockIdentifier))
            {
                this.LockIdentifier = Environment.GetEnvironmentVariable("APPLICATION_INSTANCE_ID");

                if (string.IsNullOrWhiteSpace(this.LockIdentifier))
                {
                    this.LockIdentifier = Environment.MachineName;
                }
            }
        }
    }
}
