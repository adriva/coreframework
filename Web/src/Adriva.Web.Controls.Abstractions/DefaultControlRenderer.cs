namespace Adriva.Web.Controls.Abstractions
{
    public class DefaultControlRenderer : IControlRenderer
    {
        public virtual void RenderRootControl(IControlOutputContext context)
        {

        }

        public virtual void Render(IControlOutputContext context)
        {
            if (null == context.Parent) this.RenderRootControl(context);
        }
    }
}