using System;
using System.IO;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Adriva.Common.Core;
using Adriva.Extensions.Optimization.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NUglify;
using NUglify.Html;

namespace Adriva.Extensions.Optimization.Web
{
    internal class OptimizationMiddleware : IMiddleware
    {
        private ILogger Logger;
        private readonly WebOptimizationOptions Options;
        private readonly HtmlSettings HtmlSettings;

        public OptimizationMiddleware(IOptions<WebOptimizationOptions> optionsAccessor, ILogger<OptimizationMiddleware> logger)
        {
            this.Logger = logger;
            this.Options = optionsAccessor.Value;
            this.HtmlSettings = new HtmlSettings()
            {
                IsFragmentOnly = true,
                MinifyCssAttributes = true
            };
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            this.Logger.LogInformation($"Html response minification is {(this.Options.MinifyHtml ? "Enabled" : "Disabled")}");
            if (this.Options.MinifyHtml)
            {
                Stream originalStream = context.Response.Body;

                using (AutoStream autoStream = new AutoStream(102400))
                {
                    bool fallbackToDefault = true;
                    context.Response.Body = autoStream;
                    await next.Invoke(context);
                    autoStream.Seek(0, SeekOrigin.Begin);

                    if (!string.IsNullOrWhiteSpace(context.Response.ContentType))
                    {
                        var contentType = new ContentType(context.Response.ContentType);

                        if (0 == string.Compare(contentType.MediaType, MediaTypeNames.Text.Html, StringComparison.OrdinalIgnoreCase))
                        {
                            using (StreamReader reader = new StreamReader(autoStream, Encoding.UTF8, false, 4096, true))
                            {
                                string content = await reader.ReadToEndAsync();
                                var result = Uglify.Html(content, this.HtmlSettings);

                                if (result.HasErrors)
                                {
                                    this.Logger.LogWarning("Html minification has errors. Falling back to no minification.");

                                    foreach (var error in result.Errors)
                                    {
                                        this.Logger.LogWarning(error.ToString());
                                    }
                                }

                                if (!(fallbackToDefault = result.HasErrors))
                                {
                                    context.Response.Body = originalStream;
                                    await context.Response.WriteAsync(result.Code, Encoding.UTF8);
                                    this.Logger.LogInformation($"Html minification succeeded for '{context.Request.Path}'.");
                                }
                            }
                        }
                    }

                    if (fallbackToDefault)
                    {
                        await autoStream.CopyToAsync(originalStream);
                        context.Response.Body = originalStream;
                    }
                }

            }
            else
            {
                await next.Invoke(context);
            }
        }
    }
}
