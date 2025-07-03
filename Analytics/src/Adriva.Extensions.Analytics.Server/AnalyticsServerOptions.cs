using System;

namespace Adriva.Extensions.Analytics.Server
{
    /// <summary>
    /// Defines the custom behavior of analytics server services.
    /// </summary>
    public sealed class AnalyticsServerOptions
    {
        /// <summary>
        /// Gets or sets the maximum number of items that can be stored in the server buffer.
        /// </summary>
        /// <value>An integer value that represents the number of maximum items in the server buffer.</value>
        public int BufferCapacity { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of worker threads that will process the analytics data.
        /// <remarks>The minimum number of allowed thread count is 1.</remarks>
        /// </summary>
        /// <value>An integer value represeting the number of worker threads.</value>
        public int ProcessorThreadCount { get; set; }

        /// <summary>
        /// Gets or sets the maximum time given to the repository to persist analytics data buffer.
        /// </summary>
        /// <returns>A TimeSpan value indicating the timeout.</returns>
        public TimeSpan StorageTimeout { get; set; } = TimeSpan.FromSeconds(5);
    }
}
