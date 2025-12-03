namespace Adriva.Extensions.Analytics.Server.Entities
{
    public class EventItem : IAnalyticsObject
    {
        public long Id { get; set; }

        public long AnalyticsItemId { get; set; }

        public string Name { get; set; }

        public string Environment { get; set; }

        public bool? IsDeveloperMode { get; set; }

        public AnalyticsItem AnalyticsItem { get; set; }
    }
}