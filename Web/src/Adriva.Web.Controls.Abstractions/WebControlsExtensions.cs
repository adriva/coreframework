using System;
using Adriva.Web.Controls.Abstractions;

namespace Microsoft.Extensions.DependencyInjection
{

    public class WebControlsOptions
    {
        public AssetDeliveryMethod AssetDeliveryMethod { get; set; }
    }

    public static class WebControlsServiceExtensions
    {
        public static IWebControlsBuilder AddWebControls(this IServiceCollection services, Action<WebControlsOptions> configure)
        {
            services.AddSingleton<IControlRendererFactory, DefaultControlRendererFactory>();
            services.Configure(configure);
            DefaultWebControlsBuilder builder = new DefaultWebControlsBuilder(services);
            builder.AddRenderer<DefaultControlRenderer>(Options.Options.DefaultName);
            return builder;
        }
    }
}
