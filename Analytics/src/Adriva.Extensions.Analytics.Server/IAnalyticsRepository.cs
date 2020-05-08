using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Adriva.Extensions.Analytics.Server.Entities;

namespace Adriva.Extensions.Analytics.Server
{
    public interface IAnalyticsRepository
    {

        Task StoreAsync(IEnumerable<AnalyticsItem> items, CancellationToken cancellationToken);

        Task HandleErrorAsync(IEnumerable<AnalyticsItem> items, Exception exception);
    }
}
