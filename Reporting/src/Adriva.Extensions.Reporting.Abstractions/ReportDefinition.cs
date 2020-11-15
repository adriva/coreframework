using System.Collections.Generic;
using Adriva.Common.Core;

namespace Adriva.Extensions.Reporting.Abstractions
{

    public sealed class ReportDefinition : ICloneable<ReportDefinition>
    {
        public string Base { get; set; }

        public string Name { get; set; }

        public IDictionary<string, DataSourceDefinition> DataSources { get; set; }

        public IDictionary<string, CommandDefinition> Commands { get; set; }

        public IDictionary<string, FilterDefinition> Filters { get; private set; } = new Dictionary<string, FilterDefinition>();

        public ReportDefinition Clone()
        {
            ReportDefinition clone = new ReportDefinition()
            {
                Base = this.Base,
                Name = this.Name
            };

            clone.DataSources = new Dictionary<string, DataSourceDefinition>();
            clone.Commands = new Dictionary<string, CommandDefinition>();
            clone.Filters = new Dictionary<string, FilterDefinition>();

            foreach (var child in this.DataSources) clone.DataSources.Add(child.Key, child.Value.Clone());
            foreach (var child in this.Commands) clone.Commands.Add(child.Key, child.Value.Clone());
            foreach (var child in this.Filters) clone.Filters.Add(child.Key, child.Value.Clone());

            return clone;
        }
    }
}