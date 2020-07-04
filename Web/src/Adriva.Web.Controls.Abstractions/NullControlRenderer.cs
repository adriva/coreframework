using System.Collections.Generic;
using System.Threading.Tasks;

namespace Adriva.Web.Controls.Abstractions
{
    public sealed class NullControlRenderer : IControlRenderer
    {
        public void Render(IControlOutputContext context, IDictionary<string, object> attributes)
        {
            context.Output.SuppressOutput();
        }

        public Task RenderAsync(IControlOutputContext context, IDictionary<string, object> attributes)
        {
            this.Render(context, attributes);
            return Task.CompletedTask;
        }
    }
}