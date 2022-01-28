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

        public HomeController(IReportingService reportingService)
        {
            this.ReportingService = reportingService;
        }

        public async Task<IActionResult> Index(FilterValuesDictionary values)
        {
            var def = await this.ReportingService.LoadReportDefinitionAsync("promotions/sample");
            var dd = await this.ReportingService.GetFilterDataAsync(def, "district", values);
            await this.ReportingService.PopulateFilterValuesAsync(def, null);
            return this.Content(Utilities.SafeSerialize(def));
        }
    }
}