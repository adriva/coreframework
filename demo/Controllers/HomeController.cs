using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
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
            var qm = await sm.GetBlobClientAsync("nullq");
            string t = "Hello world";
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(t);
            await qm.DeleteAsync("blog/deneme");
            return this.View();
        }
    }
}