using System;

namespace Adriva.Extensions.Reporting.Abstractions
{
    public class FilterDefinition : DynamicDefinition
    {
        public string DisplayName { get; set; }

        public TypeCode TypeCode { get; set; }

        public object DefaultValue { get; set; }
    }
}