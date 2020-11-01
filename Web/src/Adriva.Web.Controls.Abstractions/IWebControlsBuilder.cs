namespace Adriva.Web.Controls.Abstractions
{
    public interface IWebControlsBuilder
    {
        IWebControlsBuilder AddRenderer<TRenderer>() where TRenderer : class, IControlRenderer;

        IWebControlsBuilder AddRenderer<TRenderer, TEventClass>() where TRenderer : class, IControlRenderer
                                                                    where TEventClass : class, IControlRendererEvents;

        IWebControlsBuilder AddRenderer<TRenderer>(string name) where TRenderer : class, IControlRenderer;

        IWebControlsBuilder AddRenderer<TRenderer, TEventClass>(string name) where TRenderer : class, IControlRenderer
                                                                            where TEventClass : class, IControlRendererEvents;

        IWebControlsBuilder AddAssembly(string assemblyPath);
    }
}