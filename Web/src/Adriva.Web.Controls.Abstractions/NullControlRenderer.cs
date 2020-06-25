namespace Adriva.Web.Controls.Abstractions
{
    public class NullControlRenderer : IControlRenderer
    {
        public void Render(IControlOutputContext context)
        {
            context.Output.SuppressOutput();
        }
    }
}