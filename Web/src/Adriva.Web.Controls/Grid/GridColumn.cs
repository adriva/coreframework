using System.Collections.Generic;
using System.Threading.Tasks;
using Adriva.Web.Controls.Abstractions;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Adriva.Web.Controls
{
    [HtmlTargetElement("grid-column", ParentTag = "grid")]
    [RestrictChildren("column-formatter")]
    public class GridColumn : ControlTagHelper
    {
        [HtmlAttributeName("field")]
        public string Field { get; set; }

        [HtmlAttributeName("title")]
        public string Title { get; set; }

        [HtmlAttributeName("ishidden")]
        public bool IsHidden { get; set; }

        [HtmlAttributeName("width")]
        public int Width { get; set; }

        [HtmlAttributeName("minWidth")]
        public int MinWidth { get; set; }

        [HtmlAttributeName("priority")]
        public int Priority { get; set; }

        [HtmlAttributeName("alignment")]
        public HorizontalAlignment Alignment { get; set; }

        [HtmlAttributeName("format")]
        public string Format { get; set; }

        [HtmlAttributeNotBound]
        public string Formatter { get; set; }

        [HtmlAttributeNotBound]
        public IDictionary<string, string> Events { get; private set; } = new Dictionary<string, string>();

        protected override async Task ProcessAsync(IControlOutputContext context)
        {
            Grid grid = (Grid)context.Parent.Control;


            grid.Columns.Add(this);

            await Task.CompletedTask;
        }
    }
}