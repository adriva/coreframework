using System.Threading.Tasks;

namespace Adriva.Extensions.Analytics.Server.AppInsights
{
    public interface IAppInsightsValidator
    {
        ValueTask<bool> ValidateInstrumentationKeyAsync(string instrumentationKey);
    }
}
