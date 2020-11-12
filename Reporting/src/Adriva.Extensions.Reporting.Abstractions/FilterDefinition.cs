using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace Adriva.Extensions.Reporting.Abstractions
{
    public class FilterDefinition : IDynamicDefinition
    {
        public string Name { get; set; }

        public string DisplayName { get; set; }

        public TypeCode DataType { get; set; }

        public object DefaultValue { get; set; }

        public IDictionary<string, FilterDefinition> Children { get; private set; } = new Dictionary<string, FilterDefinition>();

        public IConfigurationSection Options { get; set; }

        public bool IsPredefined { get; set; }
    }
}