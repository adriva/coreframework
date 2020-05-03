using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.DependencyInjection
{
    public class AnalyticsTrackingMiddleware
    {
        private readonly RequestDelegate Next;
        private readonly ILogger Logger;

        public AnalyticsTrackingMiddleware(RequestDelegate next, ILogger<AnalyticsTrackingMiddleware> logger)
        {
            this.Next = next;
            this.Logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            await context.Response.WriteAsync($"<pre>");
            await context.Response.WriteAsync($"Method: {context.Request.Method}{Environment.NewLine}");
            await context.Response.WriteAsync($"ContentType: {context.Request.ContentType}{Environment.NewLine}");
            await context.Response.WriteAsync($"ContentLength: {context.Request.ContentLength}{Environment.NewLine}");
            await context.Response.WriteAsync($"</pre>");
        }
    }
}
