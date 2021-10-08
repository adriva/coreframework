using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Adriva.Extensions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Adriva.Worker.Host
{
    internal class ScheduledJobsEvents : IScheduledJobEvents
    {
        public Task ExecutedAsync(string instanceId, MethodInfo methodInfo, Exception error)
        {
            return Task.CompletedTask;
        }

        public Task ExecutingAsync(string instanceId, MethodInfo methodInfo)
        {
            return Task.CompletedTask;
        }
    }

    public class Startup
    {
        public Startup(IHostEnvironment hostEnvironment, IConfiguration configuration)
        {

        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(builder =>
            {
                builder.AddConsole();
            });
            // services.AddHostedService<Worker>();
            services.AddScheduledJobs();
            services.AddSingleton<IScheduledJobEvents, ScheduledJobsEvents>();
        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            using (var workerHost = WorkerHost
                    .Create(args)
                    .UseStartup<Startup>()
                    .Build())
            {
                workerHost.Run();
            }
        }
    }
}
