using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace Adriva.Extensions.Reporting.Abstractions
{
    public sealed class ReportDefinition
    {
        public string Base { get; set; }

        public string Name { get; set; }

        public IDictionary<string, DataSourceDefinition> DataSources { get; set; }

        public IDictionary<string, FilterDefinition> Filters { get; set; }
    }

    public class DataSourceDefinition
    {
        public string Type { get; set; }

        public string ConnectionString { get; set; }
    }

    public class FilterDefinition
    {
        public string DisplayName { get; set; }

        public TypeCode TypeCode { get; set; }

        public object DefaultValue { get; set; }

        public IConfigurationSection Options { get; set; }
    }
}