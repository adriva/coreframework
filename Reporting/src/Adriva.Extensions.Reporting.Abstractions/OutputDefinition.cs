using Adriva.Common.Core;

namespace Adriva.Extensions.Reporting.Abstractions
{
    public class OutputDefinition : IDataDrivenObject, ICloneable<OutputDefinition>
    {
        public string DataSource { get; set; }

        public string Command { get; set; }

        public StringKeyDictionary<FieldDefinition> Fields { get; set; }

        public OutputDefinition Clone()
        {
            OutputDefinition clone = new OutputDefinition()
            {
                DataSource = this.DataSource,
                Command = this.Command
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