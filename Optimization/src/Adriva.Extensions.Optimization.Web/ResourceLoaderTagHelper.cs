using System.IO;
using System.Text;
using System.Threading.Tasks;
using Adriva.Extensions.Caching.Abstractions;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.FileProviders;
using NUglify.JavaScript;

namespace Adriva.Extensions.Optimization.Web
{
    [HtmlTargetElement("resourceloader", TagStructure = TagStructure.NormalOrSelfClosing)]
    public class ResourceLoaderTagHelper : ITagHelper
    {
        private static readonly string Content;
        public int Order { get; set; } = 999;

        [HtmlAttributeName("inline")]
        public bool IsInline { get; set; } = true;

        static ResourceLoaderTagHelper()
        {
            var resourceFileProvider = new EmbeddedFileProvider(typeof(ResourceLoaderTagHelper).Assembly);
            var loaderFileInfo = resourceFileProvider.GetFileInfo("loader.js");

            using (var stream = loaderFileInfo.CreateReadStream())
            using (var reader = new StreamReader(stream, Encoding.UTF8, false, 4096, true))
            {
                var result = NUglify.Uglify.Js(reader.ReadToEnd(), new CodeSettings()
                {
                    Format = JavaScriptFormat.Normal,
                    AmdSupport = false,
                    IgnorePreprocessorDefines = true,
                    LocalRenaming = LocalRenaming.CrunchAll,
                    PreserveImportantComments = false,

                });
                ResourceLoaderTagHelper.Content = result.Code;
            }

        }

        public ResourceLoaderTagHelper(ICache cache)
        {

        }

        public void Init(TagHelperContext context)
        {

        }

        public async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "script";
            output.TagMode = TagMode.StartTagAndEndTag;

            if (this.IsInline)
            {
                output.Content.SetHtmlContent(ResourceLoaderTagHelper.Content);
            }

            await Task.CompletedTask;
        }
    }
}
