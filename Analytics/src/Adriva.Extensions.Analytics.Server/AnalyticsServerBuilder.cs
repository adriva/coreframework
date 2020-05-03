using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection
{
    internal sealed class AnalyticsServerBuilder : IAnalyticsServerBuilder
    {
        private readonly AnalyticsServerOptions Options;

        public AnalyticsServerBuilder(IOptions<AnalyticsServerOptions> optionsAccessor)
        {
            this.Options = optionsAccessor.Value;
        }


        public void Build(IApplicationBuilder app)
        {
            if (null == this.Options.HandlerType)
            {
                throw new ArgumentNullException("Handler", "A valid analytics handler is not specified.");
            }

            if (null == this.Options.RepositoryType)
            {
                throw new ArgumentNullException("Repository", "A valid analytics repository is not specified.");
            }

            IAnalyticsHandler handler = (IAnalyticsHandler)ActivatorUtilities.CreateInstance(app.ApplicationServices, this.Options.HandlerType);
            IAnalyticsRepository repository = (IAnalyticsRepository)ActivatorUtilities.CreateInstance(app.ApplicationServices, this.Options.RepositoryType);


        }
    }
}
