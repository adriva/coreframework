namespace Adriva.Extensions.Analytics.Abstractions
{
    public class RequestItem
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public string Environment { get; set; }

        public bool? IsDeveloperMode { get; set; }

        /// <summary>
        /// Gets or sets the duration of the request in milliseconds.
        /// </summary>
        /// <value>A double value representing the duration of the request.</value>
        public double Duration { get; set; }

        public bool IsSuccess { get; set; }

        public int ResponseCode { get; set; }

        public string Url { get; set; }

        public AnalyticsItem AnalyticsItem { get; set; }
    }
}
