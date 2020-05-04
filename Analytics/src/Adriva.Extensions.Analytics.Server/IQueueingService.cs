using System.Collections.Generic;
using Adriva.Extensions.Analytics.Server.Entities;

namespace Adriva.Extensions.Analytics.Server
{
    public interface IQueueingService
    {
        bool IsCompleted { get; }

        void Enqueue(AnalyticsItem analyticsItem);

        IEnumerable<AnalyticsItem> GetConsumingEnumerable();
    }
}
