using System;
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

        public RendererTagHelper(IControlRendererFactory rendererFactory)
        {
            this.RendererFactory = rendererFactory;
        }

        protected override Task ProcessAsync(IControlOutputContext context)
        {
            if (string.IsNullOrWhiteSpace(this.Name)) this.Name = Options.DefaultName;

            this.ControlRenderer = this.RendererFactory.GetRenderer(this.Name);
            return Task.CompletedTask;
        }
    }
}