using System;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Adriva.Analytics.Abstractions;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.ApplicationInsights;

namespace test
{
    public class BasicAnalytics_Tests
    {
        private IServiceProvider ServiceProvider;

        public BasicAnalytics_Tests()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddAnalytics(options =>
            {
                options.IsDeveloperMode = true;
                options.BacklogSize = 100;
                options.Capacity = 5;
                options.EndPointAddress = "http://no.such.domain.exists.local";
                options.InstrumentationKey = "IK01";
            });
            this.ServiceProvider = services.BuildServiceProvider();
        }

        [Fact]
        public async Task TestName()
        {
            var tc = this.ServiceProvider.GetService<TelemetryClient>();
            await Task.Run(() => Thread.Sleep(300000));
            Assert.True(true);
        }
    }
}
