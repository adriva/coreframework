using System.Collections.Concurrent;
using System.Collections.Generic;
using Adriva.Extensions.Analytics.Server.Entities;
using Microsoft.Extensions.Logging;

namespace Adriva.Extensions.Analytics.Server
{

    internal interface IQueueingService
    {
        void Enqueue(AnalyticsItem analyticsItem);

        IEnumerable<AnalyticsItem> GetConsumingEnumerable();
    }

    internal class QueueingService : IQueueingService
    {
        private readonly BlockingCollection<AnalyticsItem> Items = new BlockingCollection<AnalyticsItem>();
        private readonly ILogger<QueueingService> Logger;

        public bool IsAddingCompleted => this.Items.IsAddingCompleted;

        public QueueingService(ILogger<QueueingService> logger)
        {
            this.Logger = logger;
        }

        public void CompleteAdding()
        {
            this.Items.CompleteAdding();
        }

        public void Enqueue(AnalyticsItem analyticsItem)
        {
            if (null == analyticsItem) return;
            if (this.Items.IsCompleted) return;

            if (this.Items.TryAdd(analyticsItem, 50))
            {
                this.Logger.LogTrace("Queued analytics item");
            }
        }

        public IEnumerable<AnalyticsItem> GetConsumingEnumerable() => this.Items.GetConsumingEnumerable();
    }
}
