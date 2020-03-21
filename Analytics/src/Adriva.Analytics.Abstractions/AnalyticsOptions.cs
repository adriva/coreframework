using System;
using Microsoft.ApplicationInsights.Channel;

namespace Adriva.Analytics.Abstractions
{
    public class AnalyticsOptions
    {
        public string InstrumentationKey { get; set; }

        public string EndPointAddress { get; set; }

        public int BacklogSize { get; set; } = 1000000;

        public int Capacity { get; set; } = 500;

        public bool IsDeveloperMode { get; set; }

        public Func<ITelemetry, bool> Filter { get; set; } = _ => true;
    }
}