using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Options;

namespace Adriva.Web.Controls.Abstractions
{
    public class DefaultControlRenderer : IControlRenderer
    {
        protected virtual void RenderRootControl(IControlOutputContext context)
        {
            var v = context.Control.ViewContext.View as Microsoft.AspNetCore.Mvc.Razor.RazorView;
            v.RazorPage.SectionWriters.Add("Scripts", async () =>
            {
                (v.RazorPage as RazorPage).WriteLiteral("<script defer src='https://stackpath.bootstrapcdn.com/bootstrap/4.3.1/js/bootstrap.bundle.min.js'></script>");
                (v.RazorPage as RazorPage).WriteLiteral("<script defer src='https://unpkg.com/bootstrap-table@1.16.0/dist/bootstrap-table.min.js'></script>");
                await Task.CompletedTask;
            });
        }

        public DefaultControlRenderer(IOptions<WebControlsRendererOptions> optionsAccessor)
        {

        }

        public virtual void Render(IControlOutputContext context)
        {
            if (null == context.Parent) this.RenderRootControl(context);
        }

        public virtual Task RenderAsync(IControlOutputContext context)
        {
            if (null == context.Parent) this.RenderRootControl(context);

            return Task.CompletedTask;
        }
    }
}