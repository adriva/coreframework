using System;
using Microsoft.Extensions.DependencyInjection;

namespace Adriva.Extensions.Analytics.AppInsights
{
    internal sealed class AnalyticsBuilder : IAnalyticsBuilder
    {
        internal Action<AnalyticsOptions> ConfigureAction;
        public AnalyticsOptions Options { get; private set; }
        public IServiceCollection Services { get; private set; }

        public AnalyticsBuilder(IServiceCollection services, AnalyticsOptions options)
        {
            this.Services = services;
            this.Options = options;
        }

        public void Configure(Action<AnalyticsOptions> configure)
        {
            this.ConfigureAction = configure;
        }
    }
}
