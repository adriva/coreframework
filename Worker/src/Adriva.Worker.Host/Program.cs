using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Adriva.Extensions.Worker;
using Adriva.Extensions.Worker.Hangfire;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Adriva.Worker.Host
{
    internal class ScheduledJobsEvents : IScheduledJobEvents
    {
        public Task ExecutedAsync(object owner, string instanceId, MethodInfo methodInfo, Exception error)
        {
            return Task.CompletedTask;
        }

        public Task ExecutingAsync(object owner, string instanceId, MethodInfo methodInfo)
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
            // services.AddScheduledJobs();
            services.AddHangfireScheduledJobs();
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
                    .UseHangfire(new HangfireOptions()
                    {
                        ConnectionString = "Server=10.255.1.127\\SQL2017,1435;Database=Mrt;User Id=poasPortal;Password=portal12;MultipleActiveResultSets=True",
                        SchemaName = "jobs",
                        AutomaticRetryCount = 0
                    })
                    .Build())
            {
                workerHost.Run();
            }
        }
    }
}
