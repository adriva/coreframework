using System.Collections.Generic;
using System.Threading.Tasks;
using Adriva.Extensions.Analytics.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Adriva.Extensions.Analytics.Server
{
    public sealed class NullHandler : IAnalyticsHandler
    {
        private readonly ILogger Logger;

        public NullHandler(ILogger<NullHandler> logger)
        {
            this.Logger = logger;
        }

        public async IAsyncEnumerable<AnalyticsItem> HandleAsync(HttpRequest request)
        {
            this.Logger.LogTrace($"Null Analytics handler received: {request.Method} {request.Path} with ContentType = {request.ContentType} and ContentLength = {request.ContentLength}");
            await Task.CompletedTask;
            yield break;
        }
    }
}
