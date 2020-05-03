using System;
using Microsoft.AspNetCore.Http;

namespace Microsoft.Extensions.DependencyInjection
{
    public class AnalyticsServerOptions
    {
        public PathString BasePath { get; set; } = "/analytics";

        internal Type RepositoryType;

        internal Type HandlerType;

        public AnalyticsServerOptions UseRepository<TRepository>() where TRepository : IAnalyticsRepository
        {
            this.RepositoryType = typeof(TRepository);
            return this;
        }

        public AnalyticsServerOptions UseHandler<THandler>() where THandler : IAnalyticsHandler
        {
            this.HandlerType = typeof(THandler);
            return this;
        }
    }
}
