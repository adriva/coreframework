namespace Adriva.Extensions.Analytics.Server.Entities
{
    public class MetricItem : IAnalyticsObject
    {
        public long Id { get; set; }

        public long AnalyticsItemId { get; set; }

        public string Name { get; set; }

        public string NormalizedName { get; set; }

        public double SampleRate { get; set; }

        public DataPointType Kind { get; set; }

        public int Count { get; set; }

        public double Maximum { get; set; }

        public double Minimum { get; set; }

        public double StandardDeviation { get; set; }

        public double Value { get; set; }

        public AnalyticsItem AnalyticsItem { get; set; }
    }
}