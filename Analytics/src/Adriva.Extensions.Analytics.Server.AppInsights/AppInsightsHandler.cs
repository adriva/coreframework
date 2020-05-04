using System.Threading.Tasks;
using System.Collections.Generic;
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
using Adriva.Extensions.Analytics.Server.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace Adriva.Extensions.Analytics.Server.AppInsights
{
    public class AppInsightsHandler : IAnalyticsHandler
    {
        private readonly ILogger Logger;
        private readonly Dictionary<string, AnalyticsItemPopulator> Populators = new Dictionary<string, AnalyticsItemPopulator>();
        private static readonly JsonSerializerSettings JsonSerializerSettings;

        static AppInsightsHandler()
        {
            AppInsightsHandler.JsonSerializerSettings = new JsonSerializerSettings();
            AppInsightsHandler.JsonSerializerSettings.Converters.Add(new TelemetryConverter());
        }

        public AppInsightsHandler(IServiceProvider serviceProvider, ILogger<AppInsightsHandler> logger)
        {
            this.Logger = logger;
            var populatorServices = serviceProvider.GetServices<AnalyticsItemPopulator>();

            foreach (var populatorService in populatorServices)
            {
                this.Populators[populatorService.TargetKey] = populatorService;
            }
        }

        public async IAsyncEnumerable<AnalyticsItem> HandleAsync(HttpRequest request)
        {
            if (0 == string.Compare(request.Method, HttpMethods.Post))
            {
                using (Stream inputStream = new AutoStream(32 * 1024))
                {
                    bool hasPopulatedStream = false;

                    if (request.Headers.TryGetValue(HeaderNames.ContentType, out StringValues contentTypeHeader))
                    {
                        if (contentTypeHeader.Any(ct => 0 == string.Compare(ct, AiJsonSerializer.ContentType, StringComparison.OrdinalIgnoreCase)))
                        {
                            this.Logger.LogInformation("Analytics Middleware received compressed JSON data.");

                            using (var zipStream = new GZipStream(request.Body, CompressionMode.Decompress))
                            {
                                await zipStream.CopyToAsync(inputStream);
                                hasPopulatedStream = true;
                            }
                        }
                    }

                    if (!hasPopulatedStream)
                    {
                        await request.Body.CopyToAsync(inputStream);
                    }

                    inputStream.Seek(0, SeekOrigin.Begin);

                    this.Logger.LogInformation("Extracting envelope items from the request body.");
                    var envelopeItems = await NdJsonSerializer.DeserializeAsync<Envelope>(inputStream, AppInsightsHandler.JsonSerializerSettings);
                    this.Logger.LogInformation($"Extracted envelope items.");

                    if (envelopeItems.Any())
                    {
                        foreach (var envelopeItem in envelopeItems)
                        {
                            if (AnalyticsItemPopulator.TryPopulateItem(envelopeItem, out AnalyticsItem analyticsItem))
                            {
                                this.Logger.LogTrace($"Envelope item with data of type '{analyticsItem.Type}' is parsed.");
                                if (this.Populators.TryGetValue(analyticsItem.Type, out AnalyticsItemPopulator populator))
                                {
                                    if (populator.TryPopulate(envelopeItem, ref analyticsItem))
                                    {
                                        this.Logger.LogTrace($"Analytics item of type '{analyticsItem.Type}' is populated.");
                                    }
                                }
                                else
                                {
                                    this.Logger.LogWarning($"AppInsights handler received a type of '{analyticsItem.Type}' and doesn't have a populator registered for that type.");
                                }
                                yield return analyticsItem;
                            }
                        }
                    }
                }
            }
        }
    }
}
