using Adriva.Common.Core;
using Newtonsoft.Json.Linq;

namespace Adriva.Extensions.Reporting.Abstractions
{
    public sealed class DataSourceDefinition : IDynamicDefinition, ICloneable<DataSourceDefinition>
    {
        public string Type { get; set; }

        public string ConnectionString { get; set; }

        public JToken Options { get; set; }

        public DataSourceDefinition Clone()
        {
            return new DataSourceDefinition()
            {
                Type = this.Type,
                ConnectionString = this.ConnectionString,
                Options = this.Options?.DeepClone()
            };
        }
    }
}