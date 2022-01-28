using System.Diagnostics;

namespace Adriva.Extensions.Reporting.Abstractions
{
    [DebuggerDisplay("ReportDefinitionFile = {Name}")]
    public struct ReportDefinitionFile
    {
        public string Name { get; set; }

        public string Path { get; set; }

        public string Base { get; set; }
    }
}
