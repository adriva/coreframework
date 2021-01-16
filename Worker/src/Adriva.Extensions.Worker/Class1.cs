using System;
using Adriva.Common.Core;
using Microsoft.CSharp.RuntimeBinder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Adriva.Extensions.Worker
{
    public interface IWorkerHostBuilder
    {
        IWorkerHostBuilder UseStartup<TClass>() where TClass : class;

        void Run();
    }

    internal class WorkerHostBuilder : IWorkerHostBuilder
    {
        public IHostBuilder HostBuilder { get; private set; }

        public WorkerHostBuilder(IHostBuilder hostBuilder)
        {
            this.HostBuilder = hostBuilder;
        }

        public IWorkerHostBuilder UseStartup<TClass>() where TClass : class
        {
            this.HostBuilder.ConfigureLogging((context, builder) =>
            {
                builder.ClearProviders();
            });

            this.HostBuilder.ConfigureServices((context, services) =>
            {
                using (ServiceProvider startupServiceProvider = services.BuildServiceProvider())
                {
                    var startupClass = ActivatorUtilities.CreateInstance(startupServiceProvider, typeof(TClass));

                    var configureServicesMethod = ReflectionHelpers.FindMethod(typeof(TClass), "ConfigureServices", ClrMemberFlags.Public | ClrMemberFlags.Instance, typeof(IServiceCollection));

                    if (null == configureServicesMethod || typeof(void) != configureServicesMethod.ReturnType)
                    {
                        throw new InvalidProgramException("Invalid 'ConfigureServices' method. ConfigureServices method should be 'public', return 'void' and take one parameter of type 'Microsoft.Extensions.DependencyInjection.IServiceCollection'.");
                    }

                    configureServicesMethod.Invoke(startupClass, new[] { services });
                }
            });
            return this;
        }

        public void Run()
        {
            this.HostBuilder.Build().Run();
        }
    }

    public class WorkerHost
    {
        public static IWorkerHostBuilder Create(string[] args)
        {
            return new WorkerHostBuilder(Host.CreateDefaultBuilder(args));
        }
    }
}
