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
    public class Test : ITableRow
    {
        [NotMapped]
        public int X { get; set; }
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public string ETag { get; set; }

        public int Count { get; set; }

        public Test()
        {
            this.X = 23402;
        }

        public void ReadEntity(PropertyBag properties)
        {
            if (properties.ContainsKey("PromotionCount"))
            {
                this.Count = (int)properties["PromotionCount"];
            }
        }

        public PropertyBag WriteEntity()
        {
            throw new NotImplementedException();
        }
    }

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
            var tac = await sm.GetTableClientAsync();
            // var r = await tac.GetAsync<Test>("DomainInfo", "boyner.com.tr");
            var s = await tac.GetAllAsync<Test>(null, null, null, 1);
            var eben = s.Items.ToArray();
            return this.View();
        }
    }
}