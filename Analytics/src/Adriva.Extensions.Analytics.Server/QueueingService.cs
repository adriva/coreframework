using System.Collections.Concurrent;
using System.Collections.Generic;
using Adriva.Extensions.Analytics.Server.Entities;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Adriva.Extensions.Analytics.Server
{
    internal class QueueingService : IQueueingService
    {
        private readonly BlockingCollection<AnalyticsItem> Items = new BlockingCollection<AnalyticsItem>();
        private readonly ILogger<QueueingService> Logger;

        public bool IsCompleted => this.Items.IsCompleted;

        public QueueingService(IHostApplicationLifetime applicationLifetime, ILogger<QueueingService> logger)
        {
            this.Logger = logger;
            applicationLifetime.ApplicationStopping.Register(this.CompleteAdding);
        }

        public void CompleteAdding()
        {
            this.Items.CompleteAdding();
        }

        public void Enqueue(AnalyticsItem analyticsItem)
        {
            if (null == analyticsItem) return;
            if (this.Items.IsCompleted) return;

            if (!this.Items.TryAdd(analyticsItem, 50))
            {
                this.Logger.LogWarning("Failed to queue analytics item.");
            }
        }

        public IEnumerable<AnalyticsItem> GetConsumingEnumerable() => this.Items.GetConsumingEnumerable();
    }
}
