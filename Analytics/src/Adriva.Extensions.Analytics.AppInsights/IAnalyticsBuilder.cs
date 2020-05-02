using System;
using Microsoft.Extensions.DependencyInjection;

namespace Adriva.Extensions.Analytics.AppInsights
{
    public interface IAnalyticsBuilder
    {
        IServiceCollection Services { get; }

        AnalyticsOptions Options { get; }

        void Configure(Action<AnalyticsOptions> configure);
    }
}
