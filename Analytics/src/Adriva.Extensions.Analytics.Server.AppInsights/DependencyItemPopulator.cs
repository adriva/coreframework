using Adriva.Extensions.Analytics.Server.AppInsights.Contracts;
using Adriva.Extensions.Analytics.Server.Entities;

namespace Adriva.Extensions.Analytics.Server.AppInsights
{
    internal sealed class DependencyItemPopulator : AnalyticsItemPopulator
    {
        public override string TargetKey => "AppDependencies";

        public override bool TryPopulate(Envelope envelope, ref AnalyticsItem analyticsItem)
        {
            if (!(envelope.EventData is RemoteDependencyData dependencyData)) return false;

            DependencyItem dependencyItem = new DependencyItem();

            dependencyItem.Name = dependencyData.Name;
            dependencyItem.Duration = dependencyData.DurationInMilliseconds;
            dependencyItem.IsSuccess = dependencyData.IsSuccess;
            dependencyItem.Type = dependencyData.Type;
            dependencyItem.Target = dependencyData.Target;

            if (dependencyData.Properties.TryGetValue("DeveloperMode", out string developerModeValue) && bool.TryParse(developerModeValue, out bool isDeveloperMode)) dependencyItem.IsDeveloperMode = isDeveloperMode;
            if (dependencyData.Properties.TryGetValue("AspNetCoreEnvironment", out string environment)) dependencyItem.Environment = environment;
            if (dependencyData.Properties.TryGetValue("Input", out string input)) dependencyItem.Input = input;
            if (dependencyData.Properties.TryGetValue("Output", out string output)) dependencyItem.Output = output;

            analyticsItem.Dependencies.Add(dependencyItem);

            return true;
        }
    }
}