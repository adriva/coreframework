namespace Adriva.Extensions.Analytics.Server
{
    public interface IAnalyticsServerBuilder
    {
        IAnalyticsServerBuilder UseRepository<TRepository>() where TRepository : IAnalyticsRepository;
        IAnalyticsServerBuilder UseHandler<THandler>() where THandler : IAnalyticsHandler;
        IAnalyticsServerBuilder SetProcessorThreadCount(int threadCount);
        IAnalyticsServerBuilder SetBufferCapacity(int capacity);

        void Build();
    }
}
