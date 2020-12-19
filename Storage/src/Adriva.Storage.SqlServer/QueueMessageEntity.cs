using System;

namespace Adriva.Storage.SqlServer
{
    internal class QueueMessageEntity
    {
        public long Id { get; set; }

        public string Environment { get; set; }

        public int TimeToLive { get; set; }

        public string Content { get; set; }

        public DateTime RetrievedOnUtc { get; set; }
    }
}
