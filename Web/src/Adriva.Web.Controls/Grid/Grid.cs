using System.Collections.Generic;
using Adriva.Common.Core.Serialization.Json;
using Adriva.Web.Controls.Abstractions;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Adriva.Web.Controls
{
    [HtmlTargetElement("grid")]
    [RestrictChildren("grid-column", "grid-pager")]
    public class Grid : ControlTagHelper
    {
        protected override bool RequiresRenderer => true;

        [HtmlAttributeName("source")]
        public object DataSource { get; set; }

        [HtmlAttributeName("height")]
        public int Height { get; set; }

        [HtmlAttributeName("showdetails")]
        public bool ShowDetails { get; set; }

        [HtmlAttributeName("detailsformatter")]
        public string DetailsFormatter { get; set; }

        [HtmlAttributeName("oninitialized")]
        public string OnInitialized { get; set; }

        [HtmlAttributeNotBound]
        public IList<GridColumn> Columns { get; private set; } = new List<GridColumn>();

        [HtmlAttributeNotBound]
        public GridPager Pager { get; set; }

    }
}