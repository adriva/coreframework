using System.Collections.Generic;
using System.Threading.Tasks;
using Adriva.Extensions.Analytics.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Adriva.Extensions.Analytics.Server
{
    internal class AnalyticsTrackingMiddleware
    {
        private readonly RequestDelegate Next;
        private readonly IAnalyticsHandler Handler;
        private readonly IQueueingService QueueingService;
        private readonly ILogger Logger;

        public AnalyticsTrackingMiddleware(RequestDelegate next, IAnalyticsHandler handler, IQueueingService queueingService, ILogger<AnalyticsTrackingMiddleware> logger)
        {
            this.Next = next;
            this.Handler = handler;
            this.QueueingService = queueingService;
            this.Logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            IAsyncEnumerable<AnalyticsItem> items = this.Handler.HandleAsync(context.Request);

            if (null != items)
            {
                await foreach (var item in items)
                {
                    this.QueueingService.Enqueue(item);
                }
            }

            context.Response.StatusCode = 204;
        }
    }
}
