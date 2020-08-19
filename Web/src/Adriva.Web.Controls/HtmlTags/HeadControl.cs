using System.Threading.Tasks;
using Adriva.Web.Controls.Abstractions;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.DependencyInjection;

namespace Adriva.Web.Controls
{
    [HtmlTargetElement("head")]
    public class HeadControl : ControlTagHelper
    {
        protected override bool RequiresRenderer => false;

        protected override async Task ProcessAsync(IControlOutputContext context)
        {
            await base.ProcessAsync(context);
        }
    }
}