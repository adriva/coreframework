using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Adriva.Extensions.Analytics.Server
{
    internal sealed class AnalyticsServerBuilder : IAnalyticsServerBuilder
    {
        private readonly IServiceCollection Services;
        private Type RepositoryType;
        private Type HandlerType;

        public AnalyticsServerBuilder(IServiceCollection services)
        {
            this.Services = services;
        }

        public IAnalyticsServerBuilder UseRepository<TRepository>() where TRepository : IAnalyticsRepository
        {
            this.RepositoryType = typeof(TRepository);
            return this;
        }

        public IAnalyticsServerBuilder UseHandler<THandler>() where THandler : IAnalyticsHandler
        {
            this.HandlerType = typeof(THandler);
            return this;
        }

        public void Build()
        {
            this.Services.AddSingleton<IAnalyticsHandler>(serviceProvider =>
            {
                return (IAnalyticsHandler)ActivatorUtilities.CreateInstance(serviceProvider, this.HandlerType);
            });

            this.Services.AddSingleton<IAnalyticsRepository>(serviceProvider =>
            {
                return (IAnalyticsRepository)ActivatorUtilities.CreateInstance(serviceProvider, this.RepositoryType);
            });
        }
    }
}
