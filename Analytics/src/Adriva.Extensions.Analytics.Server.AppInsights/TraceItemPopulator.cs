using Adriva.Extensions.Analytics.Server.AppInsights.Contracts;
using Adriva.Extensions.Analytics.Server.Entities;

namespace Adriva.Extensions.Analytics.Server.AppInsights
{
    internal sealed class TraceItemPopulator : AnalyticsItemPopulator
    {
        public override string TargetKey => "AppTraces";

        public override bool TryPopulate(Envelope envelope, ref AnalyticsItem analyticsItem)
        {
            if (!(envelope.EventData is MessageData messageData)) return false;

            analyticsItem.MessageItem = new MessageItem();

            analyticsItem.MessageItem.Message = messageData.Message;
            analyticsItem.MessageItem.Severity = messageData.SeverityLevel;

            if (messageData.Properties.TryGetValue("DeveloperMode", out string developerModeValue) && bool.TryParse(developerModeValue, out bool isDeveloperMode)) analyticsItem.MessageItem.IsDeveloperMode = isDeveloperMode;
            if (messageData.Properties.TryGetValue("AspNetCoreEnvironment", out string environmentName)) analyticsItem.MessageItem.Environment = environmentName;
            if (messageData.Properties.TryGetValue("CategoryName", out string categoryName)) analyticsItem.MessageItem.Category = categoryName;

            return !string.IsNullOrWhiteSpace(analyticsItem.MessageItem.Message);
        }
    }
}