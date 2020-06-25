namespace Adriva.Web.Controls.Abstractions
{
    public class DefaultControlRenderer : IControlRenderer
    {
        public void Render(IControlOutputContext context)
        {
            if (null == context.Parent)
            {
                var nop = 3;
            }
        }
    }
}