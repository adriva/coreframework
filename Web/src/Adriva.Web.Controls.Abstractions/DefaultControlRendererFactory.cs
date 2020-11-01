using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Adriva.Web.Controls.Abstractions
{
    internal class DefaultControlRendererFactory : IControlRendererFactory
    {
        private readonly IServiceProvider ServiceProvider;
        private readonly IOptionsMonitor<WebControlsRendererOptions> OptionsMonitor;

        public DefaultControlRendererFactory(IServiceProvider serviceProvider, IOptionsMonitor<WebControlsRendererOptions> optionsMonitor)
        {
            this.ServiceProvider = serviceProvider;
            this.OptionsMonitor = optionsMonitor;
        }

        public IControlRenderer GetRenderer(string name)
        {
            var options = this.OptionsMonitor.Get(name);

            if (null == options.RendererType)
            {
                throw new InvalidOperationException($"A control renderer with name '{name}' not found. Have you forgotten to call AddRenderer<T>(string) on IWebControlsBuilder ?");
            }

            IControlRendererEvents eventClassInstance = null;

            if (null != options.EventClass)
            {
                eventClassInstance = ActivatorUtilities.CreateInstance<IControlRendererEvents>(this.ServiceProvider);
            }

            eventClassInstance = eventClassInstance ?? NullControlRendererEvents.Current;

            return (IControlRenderer)ActivatorUtilities.CreateInstance(this.ServiceProvider, options.RendererType, options, eventClassInstance);
        }
    }
}