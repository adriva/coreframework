using System;
using Hangfire;
using Microsoft.Extensions.DependencyInjection;

namespace Adriva.Extensions.Worker.Hangfire
{
    internal sealed class HangfireJobActivator : JobActivator
    {
        private readonly IServiceCollection Services;
        private readonly IServiceProvider ServiceProvider;

        public HangfireJobActivator(IServiceProvider serviceProvider, IServiceCollection services)
        {
            this.ServiceProvider = serviceProvider;
            this.Services = services;
        }

        public override JobActivatorScope BeginScope(JobActivatorContext context)
        {
            return new HangfireJobActivatorScope(this.ServiceProvider, this.Services);
        }
    }
}
