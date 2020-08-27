using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;

namespace Adriva.Web.Controls.Abstractions
{
    [HtmlTargetElement("renderer")]
    public sealed class RendererTagHelper : ControlTagHelper
    {
        private readonly IControlRendererFactory RendererFactory;

        [HtmlAttributeName("name")]
        public string Name { get; set; }

        [HtmlAttributeName("optimization-context-name")]
        public string OptimizationContextName { get; set; }

        public RendererTagHelper(IControlRendererFactory rendererFactory)
        {
            this.RendererFactory = rendererFactory;
        }

        protected override async Task ProcessAsync(IControlOutputContext context)
        {
            if (string.IsNullOrWhiteSpace(this.Name)) this.Name = Options.DefaultName;
            RendererTagAttributes rendererAttributes = new RendererTagAttributes(
                this.TagHelperContext.AllAttributes.Where(attr => 0 != string.Compare(attr.Name, nameof(this.Name), StringComparison.OrdinalIgnoreCase))
            );

            var controlRenderer = this.RendererFactory.GetRenderer(this.Name);
            await controlRenderer.RenderAsync(context, rendererAttributes);
        }
    }
}