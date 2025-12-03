using System.Collections.Generic;
using Adriva.Extensions.Analytics.Server.Entities;
using Microsoft.AspNetCore.Http;

namespace Adriva.Extensions.Analytics.Server
{
    /// <summary>
    /// Provides methods to handle the HttpRequest and extract AnalyticsItems from the request. 
    /// </summary>
    public interface IAnalyticsHandler
    {
        /// <summary>
        /// Processes the HttpRequest and extracts AnalyticsItems from it.
        /// </summary>
        /// <param name="request">Represents the current HttpRequest.</param>
        /// <returns>An IAsyncEnumerable of AnalyticsItems that stores the extracted analytics item data.</returns>
        IAsyncEnumerable<AnalyticsItem> HandleAsync(HttpRequest request);
    }
}
