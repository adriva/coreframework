using System.Threading.Tasks;
using System.Collections.Generic;
using Adriva.Extensions.Analytics.Abstractions;
using Microsoft.AspNetCore.Http;
using AiJsonSerializer = Microsoft.ApplicationInsights.Extensibility.Implementation.JsonSerializer;
using System.IO;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.IO.Compression;
using System;
using Adriva.Common.Core;
using Adriva.Extensions.Analytics.Server.AppInsights.Contracts;
using Newtonsoft.Json;

namespace Adriva.Extensions.Analytics.Server.AppInsights
{
    public class AppInsightsHandler : IAnalyticsHandler
    {
        private readonly ILogger Logger;
        private static readonly JsonSerializerSettings JsonSerializerSettings;

        static AppInsightsHandler()
        {
            AppInsightsHandler.JsonSerializerSettings = new JsonSerializerSettings();
            AppInsightsHandler.JsonSerializerSettings.Converters.Add(new TelemetryConverter());
        }

        public AppInsightsHandler(ILogger<AppInsightsHandler> logger)
        {
            this.Logger = logger;
        }

        public async Task<IEnumerable<AnalyticsItem>> HandleAsync(HttpRequest request)
        {
            await Task.CompletedTask;
            if (0 == string.Compare(request.Method, HttpMethods.Post))
            {
                using (Stream inputStream = new AutoStream(32 * 1024))
                {
                    bool isZipped = false;
                    if (request.Headers.TryGetValue(HeaderNames.ContentType, out StringValues contentTypeHeader))
                    {
                        if (contentTypeHeader.Any(ct => 0 == string.Compare(ct, AiJsonSerializer.ContentType, StringComparison.OrdinalIgnoreCase)))
                        {
                            this.Logger.LogInformation("Analytics Middleware received compressed JSON data.");
                            //inputStream = new GZipStream(request.Body, CompressionMode.Decompress);
                            using (var zipStream = new GZipStream(request.Body, CompressionMode.Decompress))
                            {
                                await zipStream.CopyToAsync(inputStream);
                            }
                        }
                    }

                    if (!isZipped)
                    {
                        await request.Body.CopyToAsync(inputStream);
                    }

                    // using (StreamReader reader = new StreamReader(inputStream))
                    // {
                    //     string j = await reader.ReadToEndAsync();
                    // }
                    inputStream.Seek(0, SeekOrigin.Begin);
                    this.Logger.LogInformation("Extracting envelope items from the request body.");
                    var envelopeItems = (await NdJsonSerializer.DeserializeAsync<Envelope>(inputStream, AppInsightsHandler.JsonSerializerSettings)).ToList();
                    this.Logger.LogInformation($"Extracted {envelopeItems.Count} envelope items.");

                    if (0 < envelopeItems.Count)
                    {

                    }
                }
            }
            return null;
        }
    }
}