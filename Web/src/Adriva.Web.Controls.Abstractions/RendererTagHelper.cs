using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Adriva.Web.Controls.Abstractions
{
    [HtmlTargetElement("renderer")]
    public sealed class RendererTagHelper : ControlTagHelper
    {
        protected override Task ProcessAsync(IControlOutputContext context)
        {
            this.ControlRenderer = new NullControlRenderer();
            return Task.CompletedTask;
        }
    }
}