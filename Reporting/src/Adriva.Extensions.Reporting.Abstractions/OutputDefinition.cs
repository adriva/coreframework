using Adriva.Common.Core;
using Newtonsoft.Json.Linq;

namespace Adriva.Extensions.Reporting.Abstractions
{
    public sealed class OutputDefinition : IDataDrivenObject, ICloneable<OutputDefinition>
    {
        public string DataSource { get; set; }

        public string Command { get; set; }

        public StringKeyDictionary<FieldDefinition> Fields { get; set; }

        public JToken Options { get; set; }

        public OutputDefinition Clone()
        {
            OutputDefinition clone = new OutputDefinition()
            {
                DataSource = this.DataSource,
                Command = this.Command,
                Options = this.Options?.DeepClone()
            };

            clone.Fields = new StringKeyDictionary<FieldDefinition>();

            if (null != this.Fields)
            {
                foreach (var entry in this.Fields)
                {
                    clone.Fields.Add(entry.Key, entry.Value?.Clone());
                }
            }

            return clone;
        }
    }
}