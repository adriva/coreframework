using System.Threading.Tasks;

namespace Adriva.Web.Controls.Abstractions
{
    public class DefaultControlRenderer : IControlRenderer
    {
        protected virtual void RenderRootControl(IControlOutputContext context)
        {

        }

        public virtual void Render(IControlOutputContext context)
        {
            if (null == context.Parent) this.RenderRootControl(context);
        }

        public Task RenderAsync(IControlOutputContext context)
        {
            if (null == context.Parent) this.RenderRootControl(context);
            return Task.CompletedTask;
        }
    }
}