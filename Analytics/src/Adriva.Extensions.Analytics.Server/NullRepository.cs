using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Adriva.Extensions.Analytics.Server.Entities;

namespace Adriva.Extensions.Analytics.Server
{
    public sealed class NullRepository : IAnalyticsRepository
    {
        public Task HandleErrorAsync(IEnumerable<AnalyticsItem> items, Exception exception)
        {
            return Task.CompletedTask;
        }

        public Task StoreAsync(IEnumerable<AnalyticsItem> items, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
