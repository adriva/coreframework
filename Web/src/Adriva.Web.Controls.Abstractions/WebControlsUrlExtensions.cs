using Adriva.Web.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc.Routing;
using System;

namespace Adriva.Web.Controls.Abstractions
{
    public static class WebControlsUrlExtensions
    {
        private static object SyncLock = new object();
        private static WebControlsOptions WebControlsOptions;
        private static IUrlHelperFactory UrlHelperFactory;

        private static void PopulateWebControlsOptionsOnce(HttpContext httpContext)
        {
            if (null == WebControlsUrlExtensions.WebControlsOptions)
            {
                lock (WebControlsUrlExtensions.SyncLock)
                {
                    if (null == WebControlsUrlExtensions.WebControlsOptions)
                    {
                        var optionsAccessor = httpContext.RequestServices.GetService<IOptions<WebControlsOptions>>();
                        WebControlsUrlExtensions.WebControlsOptions = optionsAccessor?.Value ?? new WebControlsOptions();

                        WebControlsUrlExtensions.UrlHelperFactory = httpContext.RequestServices.GetService<IUrlHelperFactory>();
                    }
                }
            }
        }

        public static string Asset(this IUrlHelper url, string resourceName, UriKind outputType = UriKind.Relative)
        {
            WebControlsUrlExtensions.PopulateWebControlsOptionsOnce(url.ActionContext.HttpContext);

            return url.ActionContext.Asset(resourceName, outputType);
        }

        public static string Asset(this ActionContext actionContext, string resourceName, UriKind outputType = UriKind.Relative)
        {
            WebControlsUrlExtensions.PopulateWebControlsOptionsOnce(actionContext.HttpContext);

            var urlHelper = WebControlsUrlExtensions.UrlHelperFactory.GetUrlHelper(actionContext);
            var assetsRoot = actionContext.HttpContext.Request.GetApplicationUri(UriKind.Relative, WebControlsUrlExtensions.WebControlsOptions.AssetsRootPath, resourceName);
            return assetsRoot.ToString();
        }
    }
}
