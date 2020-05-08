using System.Collections.Generic;
using System.Threading.Tasks;
using Adriva.Extensions.Analytics.Server.Entities;

namespace Adriva.Extensions.Analytics.Server
{
    public interface IAnalyticsRepository
    {

        Task StoreAsync(IEnumerable<AnalyticsItem> items);

    }

    public sealed class NullRepository : IAnalyticsRepository
    {
        public Task StoreAsync(IEnumerable<AnalyticsItem> items)
        {
            return Task.CompletedTask;
        }
    }
}
