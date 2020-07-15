using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Adriva.Extensions.Optimization.Abstractions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;

namespace Adriva.Extensions.Optimization.Web
{
    public class StylesheetResultTagBuilder : IOptimizationResultTagBuilder
    {
        private readonly IHttpContextAccessor HttpContextAccessor;
        private readonly WebOptimizationOptions Options;
        private readonly IWebHostEnvironment HostingEnvironment;

        public AssetFileExtension Extension => AssetFileExtension.Stylesheet;

        public StylesheetResultTagBuilder(IOptions<WebOptimizationOptions> optionsAccessor, IHttpContextAccessor httpContextAccessor, IWebHostEnvironment hostingEnvironment)
        {
            this.Options = optionsAccessor.Value;
            this.HttpContextAccessor = httpContextAccessor;
            this.HostingEnvironment = hostingEnvironment;
        }

        public async Task PopulateHtmlTagAsync(ITagBuilderOptions options, ReadOnlyTagHelperAttributeList attributeList, Asset asset, IHtmlContentBuilder output)
        {

            switch (options.Output)
            {
                case OptimizationTagOutput.Default: //same as OptimizationTagOutput.Inline
                    string content = await asset.ReadContentAsStringAsync();
                    output.AppendHtmlLine($"<style>{content}</style>");
                    break;
                case OptimizationTagOutput.StaticFile:
                case OptimizationTagOutput.Loader:

                    string webPath = null;
                    PathString relativeFilePath = new PathString(this.Options.StaticAssetsPath).Add($"/{asset.Name}");
                    var assetFileInfo = this.HostingEnvironment.WebRootFileProvider.GetFileInfo(relativeFilePath);

                    if (!assetFileInfo.Exists)
                    {
                        content = await asset.ReadContentAsStringAsync();
                        await File.WriteAllTextAsync(assetFileInfo.PhysicalPath, content, Encoding.UTF8);
                    }

                    if (string.IsNullOrWhiteSpace(this.Options.AssetRootUrl))
                    {
                        webPath = this.HttpContextAccessor.HttpContext.Request.PathBase.Add(relativeFilePath);
                    }
                    else
                    {
                        if (!Uri.TryCreate(this.Options.AssetRootUrl, UriKind.Absolute, out Uri cdnRootUri))
                        {
                            throw new UriFormatException($"'{this.Options.AssetRootUrl}' is an invalid Uri. An absolute Uri is expected.");
                        }
                        Uri.TryCreate(cdnRootUri, $"{asset.Name}", out Uri webPathUri);
                        webPath = webPathUri.ToString();
                    }

                    TagBuilder tagBuilder = new TagBuilder("link");
                    tagBuilder.TagRenderMode = TagRenderMode.SelfClosing;
                    tagBuilder.Attributes.Add("rel", "stylesheet");
                    tagBuilder.Attributes.Add("type", "text/css");
                    tagBuilder.Attributes.Add("href", webPath);

                    output.AppendHtml(tagBuilder);
                    break;
            }
        }
    }
}
