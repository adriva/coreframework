using System;
using Adriva.Extensions.Worker;
using Adriva.Extensions.Worker.Hangfire;
using Newtonsoft.Json;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class WorkerHostBuilderExtensions
    {
        public static IWorkerHostBuilder UseHangfire(this IWorkerHostBuilder builder, HangfireOptions options, Action<JsonSerializerSettings> configureSerializer = null)
        {
            builder.HostBuilder.ConfigureServices((context, services) =>
            {
                services.AddHangfire(options, configureSerializer);
            });
            return builder;
        }
    }
}
