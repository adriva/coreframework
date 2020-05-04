using System;
using System.Collections.Generic;

namespace Adriva.Extensions.Analytics.Server.Entities
{

    public class AnalyticsItem : IAnalyticsObject
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

        public string SdkVersion { get; set; }

        public string Type { get; set; }

        public string UserId { get; set; }

        public string UserAccountId { get; set; }

        public string AuthenticatedUserId { get; set; }

        public List<ExceptionItem> Exceptions { get; set; } = new List<ExceptionItem>();

        public RequestItem RequestItem { get; set; }

        public MessageItem MessageItem { get; set; }

        public List<MetricItem> Metrics { get; set; } = new List<MetricItem>();

        public List<EventItem> Events { get; set; } = new List<EventItem>();

        public List<DependencyItem> Dependencies { get; set; } = new List<DependencyItem>();

        public IDictionary<string, string> Properties => this.CustomProperties;

        public override string ToString() => $"AnalyticsItem, [{this.Type}]";
    }
}
