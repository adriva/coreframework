using System;
using Microsoft.Extensions.DependencyInjection;

namespace Adriva.Extensions.Analytics.AppInsights
{
    /// <summary>
    /// Provides methods and properties that will be used by the system to build an analytics client.
    /// </summary>
    public interface IAnalyticsBuilder
    {
        /// <summary>
        /// Gets the current service collection instance.
        /// </summary>
        /// <value>A reference to the current services collection.</value>
        IServiceCollection Services { get; }

        /// <summary>
        /// Gets the current options used by the analytics services.
        /// </summary>
        /// <value>The current instance of AnalyticsOptions class that is used by the system.</value>
        AnalyticsOptions Options { get; }

        /// <summary>
        /// Called by the system to configure the analytics options that will be used.
        /// </summary>
        /// <param name="configure">An action that is called by the system to populate analytics options data.</param>
        void Configure(Action<AnalyticsOptions> configure);
    }
}
