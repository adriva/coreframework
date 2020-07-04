using System.Collections.Generic;
using System.Threading.Tasks;

namespace Adriva.Web.Controls.Abstractions
{
    public interface IControlRenderer
    {
        void Render(IControlOutputContext context, IDictionary<string, object> attributes);

        Task RenderAsync(IControlOutputContext context, IDictionary<string, object> attributes);
    }
}