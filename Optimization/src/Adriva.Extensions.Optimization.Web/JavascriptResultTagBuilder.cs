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

        public string Extension => "js";

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

            switch (options.Output)
            {
                case OptimizationTagOutput.Default: //same as OptimizationTagOutput.Inline
                    string content = await asset.ReadContentAsStringAsync();
                    tagBuilder.InnerHtml.SetHtmlContent(content);
                    break;
                case OptimizationTagOutput.StaticFile:
                case OptimizationTagOutput.Loader:
                    PathString relativeFilePath = new PathString(this.Options.StaticAssetsPath).Add($"/{asset.Name}");
                    PathString relativeWebPath = this.HttpContextAccessor.HttpContext.Request.PathBase.Add(relativeFilePath);

                    var assetFileInfo = this.HostingEnvironment.WebRootFileProvider.GetFileInfo(relativeFilePath);
                    if (!assetFileInfo.Exists)
                    {
                        content = await asset.ReadContentAsStringAsync();
                        await File.WriteAllTextAsync(assetFileInfo.PhysicalPath, content, Encoding.UTF8);
                    }

                    tagBuilder.Attributes.Add("src", relativeWebPath);

                    if (null != asset?.Content)
                    {
                        await asset.Content.DisposeAsync();
                    }

                    break;

            }

            output.AppendHtml(tagBuilder);
        }
    }
}
