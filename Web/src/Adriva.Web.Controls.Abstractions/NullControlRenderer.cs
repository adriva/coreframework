using System;
using System.Threading.Tasks;

namespace Adriva.Web.Controls.Abstractions
{
    public sealed class NullControlRenderer : IControlRenderer
    {
        public void Render(IControlOutputContext context, RendererTagAttributes attributes)
        {
            context.Output.SuppressOutput();
        }

        public Task RenderAsync(IControlOutputContext context, RendererTagAttributes attributes)
        {
            this.Render(context, attributes);
            return Task.CompletedTask;
        }
    }
}