//#pragma warning disable ADR00001
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Adriva.Common.Core;
using Adriva.Extensions.Caching.Abstractions;
using Adriva.Extensions.Caching.Distributed;
using Adriva.Extensions.Reporting.Abstractions;
using Adriva.Storage.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace demo.Controllers
{



    [Serializable]
    public class TestDataItem
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public bool IsSelected { get; set; }

        public DateTime CreatedOn { get; set; }
    }

    public class ReportContextProvider
    {
        public DateTime GetCurrentDate()
        {
            return DateTime.UtcNow;
        }
    }

    public class HomeController : Controller
    {
        private readonly IReportingService ReportingService;
        private static readonly Random Rnd = new Random();
        Adriva.Storage.Abstractions.IStorageClientFactory SCF;

        public HomeController(IReportingService reportingService, Adriva.Storage.Abstractions.IStorageClientFactory scf)
        {
            this.ReportingService = reportingService;
            SCF = scf;
        }

        public async Task<IActionResult> Index(IDictionary<string, string> model)
        {
            var haha = await this.SCF.GetQueueClientAsync("Production");
            await haha.AddAsync(QueueMessage.Create("hello world", "no-command", QueueMessageFlags.LowPriority), TimeSpan.FromSeconds(60));
            var m = await haha.GetNextAsync(CancellationToken.None);
            await haha.DeleteAsync(m);
            // var tc = this.HttpContext.RequestServices.GetService<Microsoft.ApplicationInsights.TelemetryClient>();
            // if (null != tc)
            // {
            //     tc.TrackTrace("Hello world", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Information);
            //     tc.TrackEvent("EVENT NAME");
            //     tc.TrackAvailability("AVAILABILITY DEMO", DateTimeOffset.Now, TimeSpan.FromSeconds(10), "RUN LOCATION", true, "MESSAGE HERE");
            // }

            // var sm = this.HttpContext.RequestServices.GetService<IStorageClientFactory>();
            // var tac = await sm.GetTableClientAsync();
            // var bac = await sm.GetBlobClientAsync();
            // var pro = await bac.GetPropertiesAsync("blog/yilbasi.html");
            return this.View();
        }

        public async Task<IActionResult> Cache()
        {
            var cache = this.HttpContext.RequestServices.GetService<ICache<DistributedCache>>();
            DateTime dt = await cache.Instance.GetOrCreateAsync<DateTime>("Test001", async (entry) =>
            {
                await Task.CompletedTask;
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30);
                return DateTime.Now;
            }, "DEP01");
            return this.Content(dt.ToString());
        }

        public async Task<IActionResult> CacheChange()
        {
            var cache = this.HttpContext.RequestServices.GetService<ICache<DistributedCache>>();
            await cache.Instance.NotifyChangedAsync("DEP01");
            return this.Content("TAMAM");
        }
    }
}
#pragma warning restore ADR00001