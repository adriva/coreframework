using Adriva.Extensions.Analytics.Server.AppInsights.Contracts;
using Adriva.Extensions.Analytics.Server.Entities;

namespace Adriva.Extensions.Analytics.Server.AppInsights
{
    internal sealed class EventItemPopulator : AnalyticsItemPopulator
    {
        public override string TargetKey => "AppEvents";

        public override bool TryPopulate(Envelope envelope, ref AnalyticsItem analyticsItem)
        {
            if (!(envelope.EventData is EventData eventData)) return false;

            EventItem eventItem = new EventItem();

            eventItem.Name = eventData.Name;
            if (eventData.Properties.TryGetValue("DeveloperMode", out string developerModeValue) && bool.TryParse(developerModeValue, out bool isDeveloperMode)) eventItem.IsDeveloperMode = isDeveloperMode;
            if (eventData.Properties.TryGetValue("AspNetCoreEnvironment", out string environment)) eventItem.Environment = environment;

            analyticsItem.Events.Add(eventItem);

            return true;
        }
    }
}