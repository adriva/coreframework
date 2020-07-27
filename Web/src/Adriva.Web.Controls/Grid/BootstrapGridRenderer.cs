using Adriva.Web.Controls.Abstractions;
using Microsoft.Extensions.Options;

namespace Adriva.Web.Controls
{
    [DefaultName("bootstrap-grid")]
    public class BootstrapGridRenderer : DefaultControlRenderer
    {
        public BootstrapGridRenderer(IOptions<WebControlsRendererOptions> rendererOptionsAccessor, IOptions<WebControlsOptions> optionsAccessor)
            : base(rendererOptionsAccessor, optionsAccessor)
        {

        }

        protected override void RenderRootControl(IControlOutputContext context)
        {

        }
    }
}