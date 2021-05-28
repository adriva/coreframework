using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Adriva.Extensions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Adriva.Worker.Host
{
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
            Adriva.DevTools.CodeGenerator.CodeGeneratorServiceExtensions.AddCSharpCodeGenerator(services);
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
