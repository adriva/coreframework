using Adriva.Web.Controls.Abstractions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class WebControlsServiceExtensions
    {
        public static IWebControlsBuilder AddWebControls(this IServiceCollection services)
        {
            services.AddSingleton<IControlRendererFactory, DefaultControlRendererFactory>();
            DefaultWebControlsBuilder builder = new DefaultWebControlsBuilder(services);
            builder.AddRenderer<DefaultControlRenderer>(Options.Options.DefaultName);
            return builder;
        }
    }
}
