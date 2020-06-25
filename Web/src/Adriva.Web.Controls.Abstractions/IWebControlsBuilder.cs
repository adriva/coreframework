namespace Adriva.Web.Controls.Abstractions
{
    public interface IWebControlsBuilder
    {
        IWebControlsBuilder AddRenderer<TRenderer>(string name) where TRenderer : class, IControlRenderer;
    }
}