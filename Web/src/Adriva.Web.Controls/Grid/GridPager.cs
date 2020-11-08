using Adriva.Web.Controls.Abstractions;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Adriva.Web.Controls
{
    [HtmlTargetElement("grid-pager")]
    public class GridPager : ControlTagHelper
    {
        [HtmlAttributeName("location")]
        public ProcessLocation ProcessLocation { get; set; }

        [HtmlAttributeName("pagesizes")]
        public string PageSizes { get; set; }

        protected override void Process(IControlOutputContext context)
        {
            var grid = (Grid)context.Parent.Control;

            grid.Pager = this;
        }
    }
}