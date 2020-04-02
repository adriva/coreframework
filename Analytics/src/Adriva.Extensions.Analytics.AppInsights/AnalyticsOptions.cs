using System;
using System.Collections.Generic;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.Extensions.Logging;

namespace Adriva.Extensions.Analytics.AppInsights
{

    public class AnalyticsOptions
    {
        internal readonly IDictionary<string, LogLevel> LogLevels = new Dictionary<string, LogLevel>();

        public static string HttpClientName { get; } = "AdrivaAnalyticsHttpClient";

        public string InstrumentationKey { get; set; }

        public string EndPointAddress { get; set; }

        public int BacklogSize { get; set; } = 1000000;

        public int Capacity { get; set; } = 500;

        public bool IsDeveloperMode { get; set; }

        public Func<ITelemetry, bool> Filter { get; set; } = _ => true;

        /// <summary>
        /// Sets the logging level per category.
        /// </summary>
        /// <param name="category">The name of the category to set the logging level for. Empty string for all categories.</param>
        /// <param name="logLevel">Target logging level for the category.</param>
        /// <returns>The current instance of AnalyticsOptions class.</returns>
        public AnalyticsOptions SetLogLevel(string category, LogLevel logLevel)
        {
            this.LogLevels[category] = logLevel;
            return this;
        }
    }

}