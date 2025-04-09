using Adriva.Extensions.Analytics.Server.AppInsights.Contracts;
using Adriva.Extensions.Analytics.Server.Entities;

namespace Adriva.Extensions.Analytics.Server.AppInsights
{
    internal sealed class AvailabilityItemPopulator : AnalyticsItemPopulator
    {
        public override string TargetKey => "AppAvailabilityResults";

        public override bool TryPopulate(Envelope envelope, ref AnalyticsItem analyticsItem)
        {
            if (!(envelope.EventData is AvailabilityData availabilityData)) return false;

            AvailabilityItem availabilityItem = new AvailabilityItem();
            availabilityItem.Duration = availabilityData.Duration;
            availabilityItem.Message = availabilityData.Message;
            availabilityItem.Name = availabilityData.Name;
            availabilityItem.Success = availabilityData.Success;

            if (availabilityData.Properties.TryGetValue("AspNetCoreEnvironment", out string environment)) availabilityItem.Environment = environment;

            analyticsItem.AvailabilityItem = availabilityItem;
            return true;
        }
    }
}