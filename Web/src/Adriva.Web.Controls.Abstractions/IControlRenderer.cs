using System.Threading.Tasks;

namespace Adriva.Web.Controls.Abstractions
{
    public interface IControlRenderer
    {
        void Render(IControlOutputContext context, RendererTagAttributes attributes);

        Task RenderAsync(IControlOutputContext context, RendererTagAttributes attributes);
    }
}