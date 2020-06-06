using System;
using Adriva.Storage.Abstractions;

namespace Adriva.Storage.InMemory
{
    public sealed class InMemoryQueueMessage
    {
        public DateTimeOffset CreatedOn { get; private set; }

        public TimeSpan? InitialVisibilityTimeout { get; private set; }

        public TimeSpan? TimeToLive { get; private set; }

        public QueueMessage Message { get; private set; }

        public InMemoryQueueMessage(QueueMessage message, TimeSpan? timeToLive, TimeSpan? initialVisibilityTimeout)
        {
            this.Message = message;
            this.TimeToLive = timeToLive;
            this.InitialVisibilityTimeout = initialVisibilityTimeout;
        }
    }
}