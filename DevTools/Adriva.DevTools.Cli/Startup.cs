using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Adriva.DevTools.Cli
{
    internal class Startup
    {
        internal void ConfigureServices(IServiceCollection services)
        {
            services
                .AddLogging(builder =>
                {
                    builder.SetMinimumLevel(LogLevel.Trace);
                    builder.AddProvider(new CliLoggerProvider());
                });
        }
    }
}
