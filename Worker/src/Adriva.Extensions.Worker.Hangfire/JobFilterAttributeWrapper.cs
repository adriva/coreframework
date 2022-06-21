using System;
using Hangfire.Client;
using Hangfire.Common;
using Microsoft.Extensions.DependencyInjection;

namespace Adriva.Extensions.Worker.Hangfire
{
    internal sealed class JobFilterAttributeWrapper : JobFilterAttribute, IClientFilter
    {
        private readonly object Instance;
        private readonly IServiceProvider ServiceProvider;

        public JobFilterAttributeWrapper(IServiceProvider serviceProvider, object instance)
        {
            this.Instance = instance;
            this.ServiceProvider = serviceProvider;
        }

        public void OnCreated(CreatedContext filterContext)
        {
            using (var scope = this.ServiceProvider.CreateScope())
            {
                if (this.Instance is IClientFilter clientFilter)
                {
                    filterContext.AddServiceProvider(scope.ServiceProvider);
                    clientFilter.OnCreated(filterContext);
                }
            }
        }

        public void OnCreating(CreatingContext filterContext)
        {
            using (var scope = this.ServiceProvider.CreateScope())
            {
                if (this.Instance is IClientFilter clientFilter)
                {
                    filterContext.AddServiceProvider(scope.ServiceProvider);
                    clientFilter.OnCreating(filterContext);
                }
            }
        }
    }
}
