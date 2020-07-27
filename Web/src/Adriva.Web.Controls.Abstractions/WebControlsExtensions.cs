using System;
using Adriva.Web.Controls.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class WebControlsServiceExtensions
    {
        public static IWebControlsBuilder AddWebControls(this IServiceCollection services, Action<WebControlsOptions> configure)
        {
            services.AddSingleton<IControlRendererFactory, DefaultControlRendererFactory>();
            services.Configure(configure);
            DefaultWebControlsBuilder builder = new DefaultWebControlsBuilder(services);
            return builder;
        }

        public static IApplicationBuilder UseWebControls(this IApplicationBuilder app)
        {
            var options = app.ApplicationServices.GetService<IOptions<WebControlsOptions>>().Value;

            app.Map(options.AssetsRootPath, (applicationBuilder) =>
            {
                applicationBuilder.UseMiddleware<ResourceLoaderMiddleware>();
            });
            return app;
        }
    }
}
