using System;
using Newtonsoft.Json;

namespace Adriva.Storage.Abstractions
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class QueueMessageFlags
    {
        public static QueueMessageFlags NormalPriority = new QueueMessageFlags(0);
        public static QueueMessageFlags LowPriority = new QueueMessageFlags(1);
        public static QueueMessageFlags HighPriority = new QueueMessageFlags(2);

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
            if (0 == value) return QueueMessageFlags.NormalPriority;
            return new QueueMessageFlags(value);
        }

        public static bool operator ==(QueueMessageFlags first, QueueMessageFlags second)
        {
            if (object.ReferenceEquals(first, null) && object.ReferenceEquals(second, null)) return true;
            else if (!object.ReferenceEquals(first, null) && !object.ReferenceEquals(second, null))
            {
                return first.Value == second.Value;
            }
            else
            {
                return false;
            }
        }

        public static bool operator !=(QueueMessageFlags first, QueueMessageFlags second)
        {
            if (object.ReferenceEquals(first, null) && object.ReferenceEquals(second, null)) return false;
            else if (!object.ReferenceEquals(first, null) && !object.ReferenceEquals(second, null))
            {
                return first.Value != second.Value;
            }
            else
            {
                return true;
            }
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
