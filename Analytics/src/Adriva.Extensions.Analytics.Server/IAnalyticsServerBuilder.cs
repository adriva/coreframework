using Microsoft.AspNetCore.Http;

namespace Adriva.Extensions.Analytics.Server
{
    public interface IAnalyticsServerBuilder
    {
        IAnalyticsServerBuilder UseRepository<TRepository>() where TRepository : IAnalyticsRepository;
        IAnalyticsServerBuilder UseHandler<THandler>() where THandler : IAnalyticsHandler;

        void Build();
    }
}
