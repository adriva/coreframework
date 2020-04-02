using System;
using System.Collections.Generic;

namespace Adriva.Extensions.Analytics.Abstractions
{
    public enum Severity
    {
        //
        // Summary:
        //     Verbose severity level.
        Verbose = 0,
        //
        // Summary:
        //     Information severity level.
        Information = 1,
        //
        // Summary:
        //     Warning severity level.
        Warning = 2,
        //
        // Summary:
        //     Error severity level.
        Error = 3,
        //
        // Summary:
        //     Critical severity level.
        Critical = 4
    }

    public class AnalyticsItem
    {
        private readonly IDictionary<string, string> CustomProperties = new Dictionary<string, string>();

        public long Id { get; set; }

        public string InstrumentationKey { get; set; }

        public DateTime Timestamp { get; set; }

        public string ApplicationVersion { get; set; }

        public string RoleInstance { get; set; }

        public string OperationId { get; set; }

        public string ParentOperationId { get; set; }

        public string Ip { get; set; }

        public string Type { get; set; }

        // public List<ExceptionItem> Exceptions { get; set; } = new List<ExceptionItem>();

        // public RequestItem RequestItem { get; set; }

        public MessageItem MessageItem { get; set; }

        // public List<MetricItem> Metrics { get; set; } = new List<MetricItem>();

        // public List<EventItem> Events { get; set; } = new List<EventItem>();

        // public List<DependencyItem> Dependencies { get; set; } = new List<DependencyItem>();

        public IDictionary<string, string> Properties => this.CustomProperties;

        public override string ToString() => $"AnalyticsItem, [{this.Type}]";
    }

    public class MessageItem
    {
        public long Id { get; set; }

        public long AnalyticsItemId { get; set; }

        public string Category { get; set; }

        public string Message { get; set; }

        public string Environment { get; set; }

        public bool? IsDeveloperMode { get; set; }

        public Severity? Severity { get; internal set; }

        public AnalyticsItem AnalyticsItem { get; set; }
    }
}
