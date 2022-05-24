using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Threading;
using System.Threading.Tasks;
using Adriva.Extensions.Caching.Abstractions;
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
            string name = "tests/dbs";
            var def = await this.ReportingService.LoadReportDefinitionAsync(name);
            var o = await this.ReportingService.ExecuteReportOutputAsync(def, null);
            var stream = System.IO.File.Open("output.xlsx", System.IO.FileMode.Create, System.IO.FileAccess.ReadWrite, System.IO.FileShare.None);
            try
            {
                await this.ReportingService.RenderAsync<Adriva.Extensions.Reporting.Excel.XlsxReportRenderer>(name, model, stream);
            }
            finally
            {
                this.Response.RegisterForDispose(stream);
            }
            return this.View();
            var scf = this.HttpContext.RequestServices.GetRequiredService<IStorageClientFactory>();
            var qc = await scf.GetQueueClientAsync("zabata");

            var m = await qc.GetNextAsync(CancellationToken.None);

            if (null != m)
            {
                await qc.DeleteAsync(m);
            }

            // await qc.AddAsync(QueueMessage.Create("HEllo world", "NO_COMMAND", QueueMessageFlags.HighPriority));

            m = await qc.GetNextAsync(CancellationToken.None);

            if (null != m)
            {
                await qc.DeleteAsync(m);
            }
            return this.Content("");
        }
    }
}