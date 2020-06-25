namespace Adriva.Web.Controls.Abstractions
{
    public interface IControlRendererFactory
    {
        IControlRenderer GetRenderer(string name);
    }
}