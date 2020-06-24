using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Adriva.Web.Controls
{
    [HtmlTargetElement("grid")]
    [RestrictChildren("grid-column", "grid-pager", "grid-grouping")]
    public class Grid : ControlTagHelper
    {
        protected override async Task ProcessAsync(IControlOutputContext context)
        {
            context.Output.TagName = "div";
            context.Output.TagMode = TagMode.StartTagAndEndTag;

            await Task.CompletedTask;
        }
    }

    [HtmlTargetElement("grid-column", ParentTag = "grid")]
    [RestrictChildren("column-template")]
    public class GridColumn : ControlTagHelper
    {
        protected override async Task ProcessAsync(IControlOutputContext context)
        {
            context.Output.TagName = "div";
            context.Output.TagMode = TagMode.StartTagAndEndTag;
            context.Output.Attributes.Add("class", "col-12");
            await Task.CompletedTask;
        }
    }

    [HtmlTargetElement("column-template", ParentTag = "grid-column")]
    public class GridColumnTemplate : ControlTagHelper
    {
        protected override async Task ProcessAsync(IControlOutputContext context)
        {
            context.Output.TagName = "div";
            context.Output.TagMode = TagMode.StartTagAndEndTag;
            context.Output.Attributes.Add("class", "alert alert-warning");
            context.Output.Content.SetContent("Hello world");
            await Task.CompletedTask;
        }
    }
}