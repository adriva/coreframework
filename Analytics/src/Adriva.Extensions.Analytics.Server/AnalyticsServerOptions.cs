using System;

namespace Adriva.Extensions.Analytics.Server
{
    public sealed class AnalyticsServerOptions
    {
        public int BufferCapacity { get; set; }

        public int ProcessorThreadCount { get; set; }

        public TimeSpan StorageTimeout { get; set; } = TimeSpan.FromSeconds(5);
    }
}
