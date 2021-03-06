using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Adriva.Extensions.Analytics.Server
{
    internal sealed class AnalyticsServerBuilder : IAnalyticsServerBuilder, IConfigureOptions<AnalyticsServerOptions>
    {
        private readonly IServiceCollection ServiceCollection;
        private readonly AnalyticsServerOptions Options = new AnalyticsServerOptions();
        private Type RepositoryType;
        private Type HandlerType;

        public IServiceCollection Services => this.ServiceCollection;

        public AnalyticsServerBuilder(IServiceCollection services)
        {
            this.ServiceCollection = services;
        }

        public IAnalyticsServerBuilder UseRepository<TRepository>() where TRepository : class, IAnalyticsRepository
        {
            this.RepositoryType = typeof(TRepository);
            return this;
        }

        public IAnalyticsServerBuilder UseHandler<THandler>() where THandler : IAnalyticsHandler
        {
            this.HandlerType = typeof(THandler);
            return this;
        }

        public IAnalyticsServerBuilder SetProcessorThreadCount(int threadCount)
        {
            this.Options.ProcessorThreadCount = Math.Max(1, threadCount);
            return this;
        }

        public IAnalyticsServerBuilder SetBufferCapacity(int capacity)
        {
            this.Options.BufferCapacity = capacity;
            return this;
        }

        public IAnalyticsServerBuilder SetStorageTimeout(TimeSpan timeout)
        {
            this.Options.StorageTimeout = timeout;
            return this;
        }

        public void Build()
        {
            this.ServiceCollection.AddSingleton<IAnalyticsHandler>(serviceProvider =>
            {
                return (IAnalyticsHandler)ActivatorUtilities.CreateInstance(serviceProvider, this.HandlerType);
            });

            this.ServiceCollection.AddScoped<IAnalyticsRepository>(serviceProvider =>
            {
                if (null == this.RepositoryType)
                {
                    throw new ArgumentException($"Analytics repository type is not set.");
                }
                return (IAnalyticsRepository)ActivatorUtilities.CreateInstance(serviceProvider, this.RepositoryType);
            });

            this.ServiceCollection.ConfigureOptions(this);
        }

        public void Configure(AnalyticsServerOptions options)
        {
            options.ProcessorThreadCount = Math.Max(1, this.Options.ProcessorThreadCount);
            options.BufferCapacity = Math.Max(1, this.Options.BufferCapacity);
            options.StorageTimeout = this.Options.StorageTimeout;
        }
    }
}
