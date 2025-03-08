namespace Adriva.Extensions.Analytics.Server.Entities
{
    public class ExceptionItem : IAnalyticsObject
    {
        public long Id { get; set; }

        public long AnalyticsItemId { get; set; }

        public string Name { get; set; }

        public string RequestId { get; set; }

        public string Category { get; set; }

        public string ConnectionId { get; set; }

        public string Message { get; set; }

        public string Path { get; set; }

        public string ExceptionType { get; set; }

        public int ExceptionId { get; set; }

        public string ExceptionMessage { get; set; }

        public string StackTrace { get; set; }

        public AnalyticsItem AnalyticsItem { get; set; }

    }
}