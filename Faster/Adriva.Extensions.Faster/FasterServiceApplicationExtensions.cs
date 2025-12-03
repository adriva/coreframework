using System;
using Adriva.Extensions.Faster;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class FasterServiceApplicationExtensions
    {
        public static IApplicationBuilder UseFasterStorageApi(this IApplicationBuilder application)
        {
            var options = application.ApplicationServices.GetService<IOptions<FasterOptions>>().Value;

            application.MapWhen(httpContext =>
            {
                if (httpContext.Request.Path.StartsWithSegments(options.PathBase, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                return false;
            }, builder => builder.UseMiddleware<StorageMiddleware>());
            return application;
        }
    }
}