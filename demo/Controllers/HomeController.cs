using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
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

        public static int Get35()
        {
            return 35;
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

        public async Task<IEnumerable<object>> GetSampleData(string name, long pageIndex)
        {
            await Task.CompletedTask;
            var arr = new dynamic[10];

            for (int loop = 0; loop < 10; loop++)
            {
                arr[loop] = new ExpandoObject();
                arr[loop].Id = loop;
                arr[loop].FirstName = Guid.NewGuid().ToString();
                arr[loop].LastName = Guid.NewGuid().ToString();
            }

            return arr;
        }

        public async Task<IActionResult> Index(FilterValuesDictionary model)
        {
            var def = await this.ReportingService.LoadReportDefinitionAsync("tests/sample");
            var o = await this.ReportingService.ExecuteReportOutputAsync(def, null);
            return this.View();
        }
    }
}