using System;

namespace Adriva.Extensions.Reporting.Abstractions
{
    public class ReportContext
    {
        public ReportDefinition ReportDefinition { get; private set; }

        public object ContextProvider { get; internal set; }

        public ReportContext(ReportDefinition reportDefinition)
        {
            this.ReportDefinition = reportDefinition ?? throw new ArgumentNullException(nameof(reportDefinition));
        }
    }
}