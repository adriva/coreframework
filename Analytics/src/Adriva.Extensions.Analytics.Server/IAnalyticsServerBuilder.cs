using Microsoft.Extensions.DependencyInjection;

namespace Adriva.Extensions.Analytics.Server
{
    /// <summary>
    /// Provides methods and properties that will be used by the system to build an analytics server.
    /// </summary>
    public interface IAnalyticsServerBuilder
    {

        /// <summary>
        /// Gets the IServiceCollection that is used by the current builder.
        /// </summary>
        /// <returns>The current Microsoft.Extensions.DependencyInjection.IServiceCollection.</returns>
        IServiceCollection Services { get; }

        /// <summary>
        /// Registers an IAnalyticsRepository to be used by the system to persist analytics data.
        /// </summary>
        /// <typeparam name="TRepository">Type of the analytics repository.</typeparam>
        /// <returns>The Adriva.Extensions.Analytics.Server.IAnalyticsServerBuilder so that additional calls can be chained.</returns>
        IAnalyticsServerBuilder UseRepository<TRepository>() where TRepository : class, IAnalyticsRepository;

        /// <summary>
        /// Registers an IAnalyticsHandler to be used by the system to parse and extract analyics data from incoming requests.
        /// </summary>
        /// <typeparam name="THandler">Type of analytics handler.</typeparam>
        /// <returns>The Adriva.Extensions.Analytics.Server.IAnalyticsServerBuilder so that additional calls can be chained.</returns>
        IAnalyticsServerBuilder UseHandler<THandler>() where THandler : IAnalyticsHandler;

        /// <summary>
        /// Sets the count of processor threads that will be used to extract and persist analytics data.
        /// <remarks>Number of threads cannot be less than 1. If so, system will override the value and set it to 1 automatically.</remarks>
        /// </summary>
        /// <param name="threadCount">The number of threads that will be made available to the system.</param>
        /// <returns>The Adriva.Extensions.Analytics.Server.IAnalyticsServerBuilder so that additional calls can be chained.</returns>
        IAnalyticsServerBuilder SetProcessorThreadCount(int threadCount);

        /// <summary>
        /// Sets the maximum number of items that can be stored in the memory buffer before persisted in the repository.
        /// <remarks>The value set in this method declares an intention and not a hard limit.async System may flush the buffer before it reaches its capacity.</remarks>
        /// </summary>
        /// <param name="capacity">Maximum number of items that can be stored in the buffer.</param>
        /// <returns>The Adriva.Extensions.Analytics.Server.IAnalyticsServerBuilder so that additional calls can be chained.</returns>
        IAnalyticsServerBuilder SetBufferCapacity(int capacity);

        /// <summary>
        /// Called by the system to build the analytics server with the configuration provided.
        /// </summary>
        void Build();
    }
}
