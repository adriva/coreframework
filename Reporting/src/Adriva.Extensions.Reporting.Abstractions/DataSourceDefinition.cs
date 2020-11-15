using Adriva.Common.Core;
using Microsoft.Extensions.Configuration;

namespace Adriva.Extensions.Reporting.Abstractions
{
    public class DataSourceDefinition : IDynamicDefinition, ICloneable<DataSourceDefinition>
    {
        public string Type { get; set; }

        public string ConnectionString { get; set; }

        public IConfigurationSection Options { get; set; }

        public DataSourceDefinition Clone()
        {
            return new DataSourceDefinition()
            {
                Type = this.Type,
                ConnectionString = this.ConnectionString,
                Options = this.Options
            };
        }
    }
}