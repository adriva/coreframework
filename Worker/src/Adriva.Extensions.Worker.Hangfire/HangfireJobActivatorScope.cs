using System;
using System.Linq;
using Hangfire;
using Microsoft.Extensions.DependencyInjection;

namespace Adriva.Extensions.Worker.Hangfire
{
    internal sealed class HangfireJobActivatorScope : JobActivatorScope
    {
        private readonly IServiceScope ServiceScope;
        private readonly IServiceCollection Services;

        public HangfireJobActivatorScope(IServiceProvider serviceProvider, IServiceCollection services)
        {
            this.Services = services;
            this.ServiceScope = serviceProvider.CreateScope();
        }

        public override object Resolve(Type type)
        {
            var serviceDescriptor = this.Services.FirstOrDefault(sd => type.IsAssignableFrom(sd.ImplementationType));
            object instance = null;

            if (null != serviceDescriptor)
            {
                instance = this.ServiceScope.ServiceProvider.GetService(serviceDescriptor.ServiceType);
            }
            else
            {
                instance = ActivatorUtilities.CreateInstance(this.ServiceScope.ServiceProvider, type);
            }

            return instance;
        }

        public override void DisposeScope()
        {
            this.ServiceScope.Dispose();
        }
    }
}
