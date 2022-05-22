using System.IO;
using System.Threading.Tasks;

namespace Adriva.Extensions.Reporting.Abstractions
{
    public abstract class ReportRenderer
    {
        public virtual void Render(string title, ReportOutput output, Stream stream)
        {

        }

        public virtual async ValueTask RenderAsync(string title, ReportOutput output, Stream stream)
        {
            this.Render(title, output, stream);
            await Task.CompletedTask;
        }
    }
}