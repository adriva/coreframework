using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

namespace Adriva.Web.Controls.Abstractions
{
    internal class ResourceLoaderMiddleware
    {
        private readonly RequestDelegate Next;
        private readonly ResourceLoader Loader;
        private readonly WebControlsOptions Options;
        private readonly FileExtensionContentTypeProvider ContentTypeProvider;

        public ResourceLoaderMiddleware(RequestDelegate next, IOptions<WebControlsOptions> optionsAccessor)
        {
            this.Options = optionsAccessor.Value;
            this.Next = next;
            this.Loader = new ResourceLoader(this.Options);
            this.ContentTypeProvider = new FileExtensionContentTypeProvider();
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var resourcePath = context.Request.Path;

            if (!this.ContentTypeProvider.TryGetContentType(resourcePath, out string contentType))
            {
                contentType = "application/octet-stream";
            }

            context.Response.StatusCode = 200;
            context.Response.ContentType = contentType;
            context.Response.Headers.Add("cache-control", "public, max-age=15552000");
            try
            {
                using (var stream = this.Loader.Load(resourcePath, out string etag))
                {
                    if (!string.IsNullOrWhiteSpace(etag))
                    {

                        context.Response.Headers[HeaderNames.ETag] = $"W/\"{etag}\"";
                    }

                    await stream.CopyToAsync(context.Response.Body);
                }

            }
            catch
            {
                context.Response.StatusCode = 400;
            }
        }
    }
}