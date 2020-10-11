using System.Threading.Tasks;
using Adriva.Web.Controls.Abstractions;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Adriva.Web.Controls
{
    [HtmlTargetElement("column-formatter", ParentTag = "grid-column")]
    public class GridColumnFormatter : ControlTagHelper
    {
        protected override async Task ProcessAsync(IControlOutputContext context)
        {
            GridColumn column = (GridColumn)context.Parent.Control;
            var childContent = await context.Output.GetChildContentAsync();
            if (!childContent.IsEmptyOrWhiteSpace)
            {
                column.Formatter = childContent.GetContent()?.Trim();
            }
        }
    }
}