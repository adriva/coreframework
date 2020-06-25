using System;
using Microsoft.Extensions.DependencyInjection;

namespace Adriva.Web.Controls.Abstractions
{
    internal sealed class DefaultWebControlsBuilder : IWebControlsBuilder
    {
        private readonly IServiceCollection Services;

        public DefaultWebControlsBuilder(IServiceCollection services)
        {
            this.Services = services;
        }

        public IWebControlsBuilder AddRenderer<TRenderer>(string name) where TRenderer : class, IControlRenderer
        {
            if (null == name) throw new ArgumentNullException(nameof(name));

            this.Services.Configure<WebControlsRendererOptions>(name, options =>
            {
                options.RendererType = typeof(TRenderer);
            });
            return this;
        }
    }

    public class WebControlsRendererOptions
    {
        public Type RendererType { get; set; }
    }
}