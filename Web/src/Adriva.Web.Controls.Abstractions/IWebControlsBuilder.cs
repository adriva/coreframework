namespace Adriva.Web.Controls.Abstractions
{
    public interface IWebControlsBuilder
    {
        IWebControlsBuilder AddRenderer<TRenderer>() where TRenderer : class, IControlRenderer;

        IWebControlsBuilder AddRenderer<TRenderer>(string name) where TRenderer : class, IControlRenderer;

        IWebControlsBuilder AddAssembly(string assemblyPath);
    }
}