using System.IO;
using System.Threading.Tasks;

namespace Adriva.Extensions.Reporting.Abstractions
{
    public abstract class ReportRenderer
    {
        public virtual void Render(string title, OutputDefinition outputDefinition, ReportOutput output, Stream stream)
        {

        }

        public virtual async ValueTask RenderAsync(string title, OutputDefinition outputDefinition, ReportOutput output, Stream stream)
        {
            this.Render(title, outputDefinition, output, stream);
            await Task.CompletedTask;
        }
    }
}