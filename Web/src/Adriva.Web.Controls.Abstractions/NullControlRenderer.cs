namespace Adriva.Web.Controls.Abstractions
{
    public sealed class NullControlRenderer : IControlRenderer
    {
        public void Render(IControlOutputContext context)
        {
            context.Output.SuppressOutput();
        }
    }
}