using System;
using Adriva.Web.Controls.Abstractions;
using Microsoft.Extensions.Options;

namespace Adriva.Web.Controls
{
    [DefaultName("bootstrap-grid")]
    public class BootstrapGridRenderer : DefaultControlRenderer
    {
        public BootstrapGridRenderer(IServiceProvider serviceProvider, IOptions<WebControlsRendererOptions> rendererOptionsAccessor, IOptions<WebControlsOptions> optionsAccessor)
            : base(serviceProvider, rendererOptionsAccessor, optionsAccessor)
        {

        }

        protected override void RenderRootControl(IControlOutputContext context)
        {

        }
    }
}