using System;
using Newtonsoft.Json;

namespace Adriva.Common.Core
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class QueueMessageFlags
    {
        public static QueueMessageFlags None = new QueueMessageFlags(0);

        [JsonProperty("v")]
        public long Value { get; private set; }

        public QueueMessageFlags(long value)
        {
            this.Value = value;
        }

        public static implicit operator long(QueueMessageFlags queueMessageFlags)
        {
            return queueMessageFlags?.Value ?? 0L;
        }

        public static implicit operator QueueMessageFlags(long value)
        {
            if (0 == value) return QueueMessageFlags.None;
            return new QueueMessageFlags(value);
        }

        public override bool Equals(object obj)
        {
            return obj is QueueMessageFlags flags &&
                   this.Value == flags.Value;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this.Value);
        }

        public override string ToString()
        {
            return $"QueueMessageFlags [{this.Value}]";
        }
    }
}
