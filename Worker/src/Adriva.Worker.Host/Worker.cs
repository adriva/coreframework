using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Adriva.Extensions.Worker;
using Adriva.DevTools.CodeGenerator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Adriva.Worker.Host
{
    public class Worker : BackgroundService
    {
        private readonly IServiceProvider ServiceProvider;
        private readonly ILogger<Worker> _logger;

        public Worker(IServiceProvider serviceProvider, ILogger<Worker> logger)
        {
            this.ServiceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(1000, stoppingToken);
            }
        }

        [Schedule("20 * * * * *", RunOnStartup = true)]
        public void DoIt(CancellationToken cancellationToken)
        {
            var writer = new StringWriter();
            var codebuilder = this.ServiceProvider.GetRequiredService<Adriva.DevTools.CodeGenerator.ICodeBuilder>();
            codebuilder
                .WithNamespace("PetrolOfisi")
                .AddUsingStatement("System.Collections")
                .AddUsingStatement("System")
                .AddClass(x =>
                {
                    x
                        .WithName("Deneme")
                        .WithBaseType(typeof(System.Collections.Generic.IDictionary<string, IList<int?>>), true)
                        .WithBaseType(typeof(IEqualityComparer<int>), true)
                        .WithModifiers(DevTools.CodeGenerator.AccessModifier.Public | DevTools.CodeGenerator.AccessModifier.Sealed)
                        .WithAttribute<SerializableAttribute>(true, true, (byte)1, false)
                        ;
                })
                .AddClass(x =>
                {
                    x
                        .WithName("Deneme2")
                        .WithBaseType("Deneme")
                        .WithModifiers(DevTools.CodeGenerator.AccessModifier.Public | DevTools.CodeGenerator.AccessModifier.Partial)
                        .WithProperty(p =>
                        {
                            p
                                .WithName("UserId")
                                .HasSetter(false)
                                .WithType(typeof(long).Name)
                                .WithModifiers(DevTools.CodeGenerator.AccessModifier.Public)
                                .WithAttribute("Serializable", true, (byte)1, false)
                                ;
                        })
                        ;
                })
                .WriteTo(writer);
            ;
            writer.Flush();
            System.Console.WriteLine(writer.ToString());
            Thread.Sleep(1000);
        }
    }
}
