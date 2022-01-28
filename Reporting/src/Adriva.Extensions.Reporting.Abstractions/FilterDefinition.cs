using Adriva.Common.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json.Linq;

namespace Adriva.Extensions.Reporting.Abstractions
{
    [DebuggerDisplay("FilterDefinition = {Name}")]
    public sealed class FilterDefinition : IDataDrivenObject, IDynamicDefinition, ICloneable<FilterDefinition>
    {
        public string Name { get; set; }

        public string DisplayName { get; set; }

        public TypeCode DataType { get; set; }

        public FilterProperties Properties { get; set; }

        public object DefaultValue { get; set; }

        public string DataSource { get; set; }

        public string Command { get; set; }

        public StringKeyDictionary<FieldDefinition> Fields { get; set; } = new StringKeyDictionary<FieldDefinition>();

        public IDictionary<string, FilterDefinition> Children { get; private set; } = new Dictionary<string, FilterDefinition>();

        public JToken Options { get; set; }

        public DataSet Data { get; set; }

        public FilterDefinition Clone()
        {
            FilterDefinition clone = new FilterDefinition();
            clone.Name = this.Name;
            clone.DisplayName = this.DisplayName;
            clone.DataType = this.DataType;
            clone.Properties = this.Properties;
            clone.Options = this.Options;
            clone.DefaultValue = this.DefaultValue;
            clone.DataSource = this.DataSource;
            clone.Command = this.Command;
            clone.Options = this.Options?.DeepClone();

            foreach (var field in this.Fields)
            {
                clone.Fields.Add(field.Key, field.Value);
            }

            foreach (var child in this.Children)
            {
                clone.Children.Add(child.Key, child.Value.Clone());
            }

            return clone;
        }
    }
}