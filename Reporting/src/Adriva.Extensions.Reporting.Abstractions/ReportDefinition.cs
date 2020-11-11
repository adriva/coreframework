using System.Collections.Generic;

namespace Adriva.Extensions.Reporting.Abstractions
{

    public sealed class ReportDefinition
    {
        public string Base { get; set; }

        public string Name { get; set; }

        public IDictionary<string, DataSourceDefinition> DataSources { get; set; }

        public IDictionary<string, FilterDefinition> Filters { get; private set; } = new Dictionary<string, FilterDefinition>();
    }
}