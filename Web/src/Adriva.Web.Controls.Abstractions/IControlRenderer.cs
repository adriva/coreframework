using System.Threading.Tasks;

namespace Adriva.Web.Controls.Abstractions
{
    public interface IControlRenderer
    {
        void Render(IControlOutputContext context);

        Task RenderAsync(IControlOutputContext context);
    }
}