using System.Threading.Tasks;

namespace Adriva.Extensions.Analytics.Server.AppInsights
{
    public class AppInsightsValidator : IAppInsightsValidator
    {
        public virtual ValueTask<bool> ValidateInstrumentationKeyAsync(string instrumentationKey)
        {
            bool isValid = !string.IsNullOrWhiteSpace(instrumentationKey);
            return new ValueTask<bool>(isValid);
        }
    }
}
