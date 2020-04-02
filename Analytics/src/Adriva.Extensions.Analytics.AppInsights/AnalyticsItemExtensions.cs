using Adriva.Extensions.Analytics.Abstractions;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;

namespace Adriva.Extensions.Analytics.AppInsights
{
    internal static class TelemetryExtensions
    {
        public static AnalyticsItem ToAnalyticsItem(this ITelemetry telemetry)
        {
            AnalyticsItem analyticsItem = null;
            if (null == telemetry) return analyticsItem;

            analyticsItem = new AnalyticsItem()
            {

            };

            if (telemetry is ISupportProperties propertiesItem && null != propertiesItem.Properties)
            {
                foreach (var entry in propertiesItem.Properties)
                {
                    analyticsItem.Properties.Add(entry.Key, entry.Value);
                }
            }

            switch (telemetry)
            {
                case TraceTelemetry traceTelemetry:
                    analyticsItem.MessageItem = traceTelemetry.ToAnalyticsItem();
                    break;
            }

            return analyticsItem;
        }

        public static MessageItem ToAnalyticsItem(this TraceTelemetry traceTelemetry)
        {
            return new MessageItem()
            {
                Message = traceTelemetry.Message
            };
        }
    }

}