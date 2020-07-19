using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Adriva.Extensions.Optimization.Abstractions;
using Adriva.Web.Controls.Abstractions;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Adriva.Web.Controls
{
    [HtmlTargetElement("grid")]
    [RestrictChildren("grid-column", "grid-pager", "grid-grouping")]
    public class Grid : ControlTagHelper, IAssetProvider
    {
        protected override bool RequiresRenderer => true;

        [HtmlAttributeName("datasource")]
        public object DataSource { get; set; }

        public IEnumerable<AssetFileExtension> GetAssetFileExtensions() => new[] {
            AssetFileExtension.Javascript,
            AssetFileExtension.Stylesheet
        };

        public IEnumerable<string> GetAssetPaths(AssetFileExtension extension)
        {
            if (extension == AssetFileExtension.Javascript)
            {
                return new[]{
                    "https://stackpath.bootstrapcdn.com/bootstrap/4.3.1/js/bootstrap.bundle.min.js",
                    "https://unpkg.com/bootstrap-table@1.16.0/dist/bootstrap-table.min.js"
                };
            }
            else if (extension == AssetFileExtension.Stylesheet)
            {
                return new[]{
                    "/css/site.css"
                };
            }

            return null;
        }

        protected override async Task ProcessAsync(IControlOutputContext context)
        {
            context.Output.TagName = "div";
            context.Output.TagMode = TagMode.StartTagAndEndTag;

            await Task.CompletedTask;
        }


    }
}