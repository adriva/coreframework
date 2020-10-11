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
    public class Grid : ControlTagHelper
    {
        protected override bool RequiresRenderer => true;

        [HtmlAttributeName("source")]
        public string DataSource { get; set; }

        [HtmlAttributeName("height")]
        public int Height { get; set; }

        [HtmlAttributeNotBound]
        public IList<GridColumn> Columns { get; private set; } = new List<GridColumn>();

    }
}