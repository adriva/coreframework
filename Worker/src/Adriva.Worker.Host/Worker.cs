using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Adriva.Extensions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Adriva.Worker.Host
{
    public class WorkerBackgroundService : IDisposable
    {
        public WorkerBackgroundService(IServiceProvider sp)
        {

        }

        [Schedule("0 */1 * * * *", RunOnStartup = false)]
        [Adriva.Extensions.Worker.Hangfire.IgnoreMissedRun(15)]
        public void DoIt(CancellationToken cancellationToken)
        {
            System.Console.WriteLine($"DOOO => {DateTime.Now}");
        }

        public void Dispose()
        {
            System.Console.WriteLine("D");
        }
    }
}
