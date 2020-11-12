using Microsoft.Extensions.Configuration;

namespace Adriva.Extensions.Reporting.Abstractions
{
    public class DataSourceDefinition : IDynamicDefinition
    {
        public string Type { get; set; }

        public string ConnectionString { get; set; }

        public IConfigurationSection Options { get; set; }
    }
}