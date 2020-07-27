using Adriva.Web.Controls.Abstractions;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Adriva.Web.Controls
{
    [HtmlTargetElement("head")]
    public class HeadControl : ControlTagHelper
    {
        protected override bool RequiresRenderer => false;
    }
}