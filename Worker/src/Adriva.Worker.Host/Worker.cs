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

        [Schedule("*/20 * * * * *", RunOnStartup = true)]
        public void DoIt(CancellationToken cancellationToken)
        {
            throw new Exception("adada");
        }

        public void Dispose()
        {
            System.Console.WriteLine("D");
        }
    }
}
