using System;

namespace Adriva.Extensions.Analytics.Server.Entities
{
    public class AvailabilityItem : IAnalyticsObject
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public TimeSpan? Duration { get; set; }

        public bool Success { get; set; }

        public string Environment { get; set; }
        public string Message { get; set; }
    }
}