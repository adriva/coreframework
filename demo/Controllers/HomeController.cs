using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Adriva.Common.Core;
using Adriva.Storage.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace demo.Controllers
{
    public class HomeController : Controller
    {
        private static readonly Random Rnd = new Random();

        public HomeController()
        {
        }

        public async Task<IActionResult> Index()
        {
            var tc = this.HttpContext.RequestServices.GetService<Microsoft.ApplicationInsights.TelemetryClient>();
            if (null != tc)
            {
                tc.TrackTrace("Hello world", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Information);
                tc.TrackEvent("EVENT NAME");
                tc.TrackAvailability("AVAILABILITY DEMO", DateTimeOffset.Now, TimeSpan.FromSeconds(10), "RUN LOCATION", true, "MESSAGE HERE");
            }
            var sm = this.HttpContext.RequestServices.GetService<IStorageClientFactory>();
            var mm = await sm.GetBlobClientAsync();
            string token = null;

            SegmentedResult<string> sr = SegmentedResult<string>.Empty;
            do
            {
                sr = await mm.ListAllAsync(sr.ContinuationToken, null, 50);
            } while (sr.HasMore);
            return this.View();
        }
    }
}