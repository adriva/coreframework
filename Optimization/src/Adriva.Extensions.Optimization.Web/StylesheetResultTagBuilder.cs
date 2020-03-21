using System.IO;
using System.Text;
using System.Threading.Tasks;
using Adriva.Extensions.Optimization.Abstractions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Adriva.Extensions.Optimization.Web
{
    public class StylesheetResultTagBuilder : IOptimizationResultTagBuilder
    {
        private readonly IHttpContextAccessor HttpContextAccessor;
        private readonly WebOptimizationOptions Options;
        private readonly IHostingEnvironment HostingEnvironment;

        public string Extension => "css";

        public StylesheetResultTagBuilder(IOptions<WebOptimizationOptions> optionsAccessor, IHttpContextAccessor httpContextAccessor, IHostingEnvironment hostingEnvironment)
        {
            this.Options = optionsAccessor.Value;
            this.HttpContextAccessor = httpContextAccessor;
            this.HostingEnvironment = hostingEnvironment;
        }

        public async Task PopulateHtmlTagAsync(ITagBuilderOptions options, Asset asset, IHtmlContentBuilder output)
        {

            switch (options.Output)
            {
                case OptimizationTagOutput.Default: //same as OptimizationTagOutput.Inline
                    string content = await asset.ReadContentAsStringAsync();
                    output.AppendHtmlLine($"<style>{content}</style>");
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

                    output.AppendHtmlLine($"<link rel=\"stylesheet\" type=\"text/css\" href=\"{relativeWebPath}\">");
                    break;
            }
        }
    }
}
