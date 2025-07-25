namespace Adriva.Extensions.Analytics.Server.Entities
{
    public class MessageItem : IAnalyticsObject
    {
        public long Id { get; set; }

        public long AnalyticsItemId { get; set; }

        public string Category { get; set; }

        public string Message { get; set; }

        public string Environment { get; set; }

        public bool? IsDeveloperMode { get; set; }

        public Severity? Severity { get; set; }

        public AnalyticsItem AnalyticsItem { get; set; }
    }
}
