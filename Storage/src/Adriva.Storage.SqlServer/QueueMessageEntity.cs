using System;

namespace Adriva.Storage.SqlServer
{
    internal class QueueMessageEntity
    {
        public long Id { get; set; }

        public string Environment { get; set; }

        public int VisibilityTimeout { get; set; }

        public byte[] Content { get; set; }

        public DateTime TimestampUtc { get; set; }

        public DateTime RetrievedOnUtc { get; set; }

    }
}
