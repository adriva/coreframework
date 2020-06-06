using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Adriva.Storage.Abstractions;

namespace Adriva.Storage.InMemory
{
    public class InMemoryQueue
    {
        private ConcurrentQueue<InMemoryQueueMessage> Queue = new ConcurrentQueue<InMemoryQueueMessage>();

        public virtual void Enqueue(QueueMessage message, TimeSpan? timeToLive, TimeSpan? initialVisibilityDelay)
        {
            if (null == message) return;

            this.Queue.Enqueue(new InMemoryQueueMessage(message, timeToLive, initialVisibilityDelay));
        }

        public virtual bool TryDequeue(out QueueMessage message)
        {
            message = null;

            while (this.Queue.TryDequeue(out InMemoryQueueMessage tempMessage))
            {
                bool isValidMessage = true;

                if (tempMessage.TimeToLive.HasValue)
                {
                    if (DateTimeOffset.Now > tempMessage.CreatedOn.Add(tempMessage.TimeToLive.Value))
                    {
                        isValidMessage = false;
                    }
                }

                if (isValidMessage)
                {
                    if (tempMessage.InitialVisibilityTimeout.HasValue)
                    {
                        if (DateTimeOffset.Now < tempMessage.CreatedOn.Add(tempMessage.InitialVisibilityTimeout.Value))
                        {
                            isValidMessage = false;
                            this.Queue.Enqueue(tempMessage);
                        }
                    }
                }

                if (isValidMessage)
                {
                    message = tempMessage.Message;
                    return true;
                }
            }

            return false;
        }
    }

    public class InMemoryQueueClient : IQueueClient
    {
        private readonly InMemoryQueue InMemoryQueue;

        public InMemoryQueueClient(InMemoryQueue inMemoryQueue)
        {
            this.InMemoryQueue = inMemoryQueue;
        }

        public ValueTask InitializeAsync(string clientName) => new ValueTask();

        public ValueTask AddAsync(QueueMessage message, TimeSpan? timeToLive = null, TimeSpan? initialVisibilityDelay = null)
        {
            this.InMemoryQueue.Enqueue(message, timeToLive, initialVisibilityDelay);
            return new ValueTask();
        }

        public Task<QueueMessage> GetNextAsync(CancellationToken cancellationToken)
        {
            if (this.InMemoryQueue.TryDequeue(out QueueMessage message)) return Task.FromResult(message);
            return Task.FromResult((QueueMessage)null);
        }

        public ValueTask DisposeAsync() => new ValueTask();
    }
}