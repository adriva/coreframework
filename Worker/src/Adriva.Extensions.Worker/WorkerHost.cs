using System;
using Microsoft.Extensions.Hosting;

namespace Adriva.Extensions.Worker
{
    public sealed class WorkerHost : IDisposable
    {
        private readonly IHost InnerHost;

        public static IWorkerHostBuilder Create(string[] args)
        {
            return new WorkerHostBuilder(Host.CreateDefaultBuilder(args));
        }

        internal WorkerHost(IHost host)
        {
            this.InnerHost = host;
        }

        public void Run()
        {
            this.InnerHost.Run();
        }

        public void Dispose()
        {
            this.InnerHost.Dispose();
        }
    }
}
