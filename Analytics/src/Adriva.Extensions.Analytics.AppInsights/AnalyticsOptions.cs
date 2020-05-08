using System;
using System.Collections.Generic;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.Extensions.Logging;

namespace Adriva.Extensions.Analytics.AppInsights
{

    /// <summary>
    /// Defines the custom behavior of analytics services.
    /// </summary>
    public class AnalyticsOptions
    {
        internal readonly IDictionary<string, LogLevel> LogLevels = new Dictionary<string, LogLevel>();

        /// <summary>
        /// Gets the name of the HttpClient instance used to communicate with analytics server.
        /// </summary>
        /// <value></value>
        public static string HttpClientName { get; } = "AdrivaAnalyticsHttpClient";

        /// <summary>
        /// Gets or sets the instrumentation key that uniquely identifies the application.
        /// </summary>
        /// <value>A string value that represents the instrumentation key.</value>
        public string InstrumentationKey { get; set; }

        /// <summary>
        /// Gets or sets the address of the server that the analytics data will be sent to.
        /// </summary>
        /// <value>A string that represents the URI of the analytics server.</value>
        public string EndPointAddress { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of items that can be stored in the backlog.
        /// </summary>
        /// <value>An integer value that represents the number of maximum items in the backlog.</value>
        public int BacklogSize { get; set; } = 100000;

        /// <summary>
        /// Gets or sets the maximum number of items that can be stored in the client buffer.
        /// </summary>
        /// <value>An integer value that represents the number of maximum items in the client buffer.</value>
        public int Capacity { get; set; } = 500;

        /// <summary>
        /// Gets or sets a value indicating if the current analytics client is running in developer mode.
        /// </summary>
        /// <value>A boolean value that represents if the current analytics client is in developer mode.</value>
        public bool IsDeveloperMode { get; set; }

        public int TransmitThreadCount { get; set; } = Environment.ProcessorCount;

        public Func<ITelemetry, bool> Filter { get; set; } = _ => true;

        public Action<IServiceProvider, ITelemetry> Initializer { get; set; } = (sp, t) => { };

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