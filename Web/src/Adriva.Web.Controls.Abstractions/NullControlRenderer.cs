using System.Threading.Tasks;

namespace Adriva.Web.Controls.Abstractions
{
    public sealed class NullControlRenderer : IControlRenderer
    {
        public void Render(IControlOutputContext context)
        {
            context.Output.SuppressOutput();
        }

        public Task RenderAsync(IControlOutputContext context)
        {
            this.Render(context);
            return Task.CompletedTask;
        }
    }
}