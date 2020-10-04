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
                    "bootstrap.bundle.js",
                    "bootstrap.table.js"
                };
            }
            else if (extension == AssetFileExtension.Stylesheet)
            {
                return null;
            }

            return null;
        }
    }
}