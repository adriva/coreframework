using System.Threading.Tasks;
using Adriva.Web.Controls.Abstractions;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Adriva.Web.Controls
{
    [HtmlTargetElement("column-template", ParentTag = "grid-column")]
    public class GridColumnTemplate : ControlTagHelper
    {
        protected override async Task ProcessAsync(IControlOutputContext context)
        {
            GridColumn parentColumn = (GridColumn)context.Parent.Control;
            context.Output.SuppressOutput();

            var innerContent = await context.Output.GetChildContentAsync(true);
            parentColumn.Template = innerContent.GetContent()?.Trim();
        }
    }
}