using System;
using Microsoft.AspNetCore.Http;

namespace Adriva.Web.Common
{
    public static class HttpContextExtensions
    {
        public static Uri GetApplicationUri(this HttpRequest request, UriKind outputType, params string[] paths)
        {

            if (null == request) throw new ArgumentNullException(nameof(request));

            PathString normalizedPath = new PathString(string.Empty);

            if (null != paths && 0 < paths.Length)
            {
                for (int loop = 0; loop < paths.Length; loop++)
                {
                    if (!string.IsNullOrWhiteSpace(paths[loop]))
                    {
                        if (!paths[loop].StartsWith("/"))
                        {
                            paths[loop] = $"/{paths[loop]}";
                        }

                        normalizedPath = normalizedPath.Add(paths[loop]);
                    }
                }
            }

            UriBuilder builder = new UriBuilder(request.Scheme, request.Host.Host);
            builder.Port = request.Host.Port ?? -1;
            builder.Path = request.PathBase.Add(normalizedPath);

            switch (outputType)
            {
                case UriKind.Absolute:
                    return builder.Uri;
                default:
                    var requestUri = request.GetApplicationUri(UriKind.Absolute, request.Path);
                    return requestUri.MakeRelativeUri(builder.Uri);
            }

        }
    }
}