using System;

namespace Adriva.Storage.SqlServer
{
    internal class QueueMessageEntity
    {
        public long Id { get; set; }

        public string Environment { get; set; }

        public string Application { get; set; }

        public int VisibilityTimeout { get; set; }

        public int TimeToLive { get; set; }

        public string Content { get; set; }

        public string Command { get; set; }

        public long Flags { get; set; }

        public DateTime TimestampUtc { get; set; }

        public DateTime? RetrievedOnUtc { get; set; }

    }
}
