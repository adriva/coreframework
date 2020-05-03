using System.Collections.Generic;
using System.Threading.Tasks;
using Adriva.Extensions.Analytics.Abstractions;
using Microsoft.AspNetCore.Http;

namespace Adriva.Extensions.Analytics.Server
{
    public interface IAnalyticsHandler
    {
        Task<IEnumerable<AnalyticsItem>> HandleAsync(HttpRequest request);
    }
}
