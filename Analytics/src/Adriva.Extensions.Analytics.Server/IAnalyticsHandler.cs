using System.Collections.Generic;
using Adriva.Extensions.Analytics.Server.Entities;
using Microsoft.AspNetCore.Http;

namespace Adriva.Extensions.Analytics.Server
{
    public interface IAnalyticsHandler
    {
        IAsyncEnumerable<AnalyticsItem> HandleAsync(HttpRequest request);
    }
}
