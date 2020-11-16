using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Adriva.Common.Core;
using Adriva.Extensions.Reporting.Abstractions;
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

        public HomeController(IReportingService reportingService)
        {
            this.ReportingService = reportingService;
        }

        public async Task<IActionResult> Index(IDictionary<string, string> model)
        {
            var rd = await this.ReportingService.LoadReportDefinitionAsync("promotions/sample");
            await this.ReportingService.ExecuteReportOutputAsync(rd, model);
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
    }
}