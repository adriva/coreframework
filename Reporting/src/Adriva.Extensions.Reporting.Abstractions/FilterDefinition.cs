using Adriva.Common.Core;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace Adriva.Extensions.Reporting.Abstractions
{
    public class FilterDefinition : IDynamicDefinition, ICloneable<FilterDefinition>
    {
        public string Name { get; set; }

        public string DisplayName { get; set; }

        public TypeCode DataType { get; set; }

        public FilterType Type { get; set; }

        public object DefaultValue { get; set; }

        public IDictionary<string, FilterDefinition> Children { get; private set; } = new Dictionary<string, FilterDefinition>();

        public IConfigurationSection Options { get; set; }

        public FilterDefinition Clone()
        {
            FilterDefinition clone = new FilterDefinition();
            clone.Name = this.Name;
            clone.DisplayName = this.DisplayName;
            clone.DataType = this.DataType;
            clone.Type = this.Type;
            clone.Options = this.Options;
            clone.DefaultValue = this.DefaultValue;

            foreach (var child in this.Children)
            {
                clone.Children.Add(child.Key, child.Value.Clone());
            }

            return clone;
        }
    }
}