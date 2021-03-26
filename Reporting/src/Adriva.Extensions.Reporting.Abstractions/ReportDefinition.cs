using Adriva.Common.Core;

namespace Adriva.Extensions.Reporting.Abstractions
{

    public sealed class ReportDefinition : ICloneable<ReportDefinition>
    {
        public string Base { get; set; }

        public string Name { get; set; }

        public string ContextProvider { get; set; }

        public StringKeyDictionary<DataSourceDefinition> DataSources { get; set; }

        public StringKeyDictionary<CommandDefinition> Commands { get; set; }

        public FilterDefinitionDictionary Filters { get; set; }

        public OutputDefinition Output { get; set; }

        public ReportDefinition Clone()
        {
            ReportDefinition clone = new ReportDefinition()
            {
                Base = this.Base,
                Name = this.Name
            };

            clone.ContextProvider = this.ContextProvider;
            clone.DataSources = new StringKeyDictionary<DataSourceDefinition>();
            clone.Commands = new StringKeyDictionary<CommandDefinition>();
            clone.Filters = new FilterDefinitionDictionary();

            foreach (var child in this.DataSources) clone.DataSources.Add(child.Key, child.Value?.Clone());
            foreach (var child in this.Commands) clone.Commands.Add(child.Key, child.Value?.Clone());
            foreach (var child in this.Filters) clone.Filters.Add(child.Key, child.Value?.Clone());

            clone.Output = (this.Output ?? new OutputDefinition()).Clone();

            return clone;
        }
    }

    public sealed class ReportOutput
    {
        public ReportCommand Command { get; private set; }

        public DataSet DataSet { get; private set; }

        public ReportOutput(ReportCommand reportCommand, DataSet dataSet)
        {
            this.Command = reportCommand;
            this.DataSet = dataSet;
        }
    }
}