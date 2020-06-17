using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Adriva.Common.Core;
using Adriva.Storage.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.DependencyInjection;

namespace demo.Controllers
{
    public class Test : ITableItem
    {
        [NotMapped]
        public string DomainName
        {
            get => this.RowKey;
            set => this.RowKey = value;
        }

        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public string ETag { get; set; }

        public int PromotionCount { get; set; }

        public void ReadEntity(IDictionary<string, EntityProperty> properties, OperationContext operationContext)
        {

        }

        public IDictionary<string, EntityProperty> WriteEntity(OperationContext operationContext)
        {
            return new Dictionary<string, EntityProperty>();
        }
    }

    public class HomeController : Controller
    {
        private static readonly Random Rnd = new Random();

        public int X { get; set; } = 0;

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
            await tac.UpsertAsync(new Test() { PartitionKey = "PK", RowKey = "RK", PromotionCount = 99, ETag = "non" });
            return this.View();
        }
    }
}