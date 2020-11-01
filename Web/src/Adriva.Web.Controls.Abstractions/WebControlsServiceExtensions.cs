using System;
using System.Linq;
using Adriva.Extensions.Optimization.Web;
using Adriva.Web.Controls.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class WebControlsServiceExtensions
    {
        public static IWebControlsBuilder AddWebControls(this IServiceCollection services, Action<WebControlsOptions> configure)
        {
            Type type = typeof(IOptimizationResultTagBuilder);
            if (null == services.FirstOrDefault(x => type.IsAssignableFrom(x.ServiceType)))
            {
                throw new InvalidOperationException($"Web controls requires the web optimization services to be registered first. Did you forget to call services.AddOptimization(...) method from Adriva.Extensions.Optimization.Web library?");
            }
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
