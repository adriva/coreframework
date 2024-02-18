using System.IO;

namespace Adriva.Extensions.Reporting.Abstractions
{
    public sealed class NullReportRenderer : ReportRenderer
    {
        public override void Render(string title, OutputDefinition outputDefinition, ReportOutput output, Stream stream)
        {

        }
    }
}