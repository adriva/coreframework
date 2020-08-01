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
    public class JavascriptResultTagBuilder : IOptimizationResultTagBuilder
    {
        private readonly IHttpContextAccessor HttpContextAccessor;
        private readonly WebOptimizationOptions Options;
        private readonly IWebHostEnvironment HostingEnvironment;

        public AssetFileExtension Extension => AssetFileExtension.Javascript;

        public JavascriptResultTagBuilder(IOptions<WebOptimizationOptions> optionsAccessor, IHttpContextAccessor httpContextAccessor, IWebHostEnvironment hostingEnvironment)
        {
            this.Options = optionsAccessor.Value;
            this.HttpContextAccessor = httpContextAccessor;
            this.HostingEnvironment = hostingEnvironment;
        }

        public async Task PopulateHtmlTagAsync(ITagBuilderOptions options, ReadOnlyTagHelperAttributeList attributeList, Asset asset, IHtmlContentBuilder output)
        {

            TagBuilder tagBuilder = new TagBuilder("script");
            tagBuilder.TagRenderMode = TagRenderMode.Normal;
            tagBuilder.Attributes.Add("type", "text/javascript");

            if (attributeList.ContainsName("defer")) tagBuilder.Attributes.Add("defer", null);
            else if (attributeList.ContainsName("async")) tagBuilder.Attributes.Add("async", null);

            var outputMode = options.Output;

            if (!this.Options.BundleJavascripts && !this.Options.MinifyJavascripts && OptimizationTagOutput.Loader != outputMode) outputMode = OptimizationTagOutput.Tag;
            else if (OptimizationTagOutput.Tag == outputMode && (this.Options.BundleStylesheets || this.Options.MinifyStylesheets)) outputMode = OptimizationTagOutput.StaticFile;

            switch (outputMode)
            {
                case OptimizationTagOutput.Default: //same as OptimizationTagOutput.Inline
                    string content = await asset.ReadContentAsStringAsync();
                    tagBuilder.InnerHtml.SetHtmlContent(content);
                    break;
                case OptimizationTagOutput.Tag:
                    tagBuilder.Attributes.Add("src", asset.GetWebLocation(this.Options));
                    if (null != asset?.Content) await asset.Content.DisposeAsync();
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

                    if (OptimizationTagOutput.StaticFile == outputMode)
                    {
                        tagBuilder.Attributes.Add("src", webPath);
                    }
                    else
                    {
                        tagBuilder.InnerHtml.SetHtmlContent($"adriva.loader.pushScript('{webPath}')");
                    }

                    if (null != asset?.Content) await asset.Content.DisposeAsync();
                    break;

            }

            output.AppendHtml(tagBuilder);
        }
    }
}
