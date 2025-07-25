using System.Diagnostics;
using Adriva.Common.Core;
using Newtonsoft.Json.Linq;

namespace Adriva.Extensions.Reporting.Abstractions
{
    [DebuggerDisplay("ReportDefinition = {Name} [Base = {Base}]")]
    public sealed class ReportDefinition : ICloneable<ReportDefinition>, IDynamicDefinition
    {
        public string Base { get; set; }

        public string Name { get; set; }

        public string ContextProvider { get; set; }

        public string PostProcessor { get; set; }

        public StringKeyDictionary<DataSourceDefinition> DataSources { get; set; }

        public StringKeyDictionary<CommandDefinition> Commands { get; set; }

        public FilterDefinitionDictionary Filters { get; set; }

        public OutputDefinition Output { get; set; }

        public JToken Options { get; set; }

        public ReportDefinition Clone()
        {
            ReportDefinition clone = new()
            {
                Base = this.Base,
                Name = this.Name,
                ContextProvider = this.ContextProvider,
                PostProcessor = this.PostProcessor,
                DataSources = [],
                Commands = [],
                Filters = []
            };

            foreach (var child in this.DataSources) clone.DataSources.Add(child.Key, child.Value?.Clone());
            foreach (var child in this.Commands) clone.Commands.Add(child.Key, child.Value?.Clone());
            foreach (var child in this.Filters) clone.Filters.Add(child.Key, child.Value?.Clone());

            clone.Output = (this.Output ?? new OutputDefinition()).Clone();
            clone.Options = this.Options?.DeepClone();

            return clone;
        }
    }
}