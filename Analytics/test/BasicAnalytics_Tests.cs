using System;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Adriva.Extensions.Analytics.AppInsights;
using Microsoft.Extensions.Logging;
using Microsoft.ApplicationInsights.Channel;
using System.Linq;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights;

namespace test
{
    public class BasicAnalytics_Tests
    {
        private readonly IServiceProvider ServiceProvider;
        private readonly TelemetryClient TelemetryClient;
        private readonly PersistentChannel TelemetryChannel;

        public BasicAnalytics_Tests()
        {
            IServiceCollection services = new ServiceCollection();

            services.AddGenericAnalytics(options =>
            {
                options.IsDeveloperMode = true;
                options.BacklogSize = 100;
                options.Capacity = 50;
                options.SetLogLevel(string.Empty, LogLevel.Trace);
                options.EndPointAddress = "http://www.some.url.here.com";
            });

            this.ServiceProvider = services.BuildServiceProvider();
            this.TelemetryClient = this.ServiceProvider.GetService<TelemetryClient>();
            this.TelemetryChannel = (PersistentChannel)this.ServiceProvider.GetService<ITelemetryChannel>();
        }

        [Fact]
        public void EnsureAllTracesAreInBuffer()
        {
            var loggingFactory = this.ServiceProvider.GetService<ILoggerFactory>();
            var logger = loggingFactory.CreateLogger("BasicAnalytics_Tests");

            logger.LogTrace("Trace Log Example");
            logger.LogInformation("Information Log Example");
            logger.LogDebug("Debug Log Example");
            logger.LogWarning("Warning Log Example");
            logger.LogError("Error Log Example");
            logger.LogCritical("Critical Log Example");

            Assert.True(6 == this.TelemetryChannel.Buffer.TelemetryItems.OfType<TraceTelemetry>().Count());
            this.TelemetryChannel.Flush();
        }
    }
}
