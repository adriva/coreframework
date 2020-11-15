using Adriva.Common.Core;

namespace Adriva.Extensions.Reporting.Abstractions
{
    public class OutputDefinition : IDataDrivenObject, ICloneable<OutputDefinition>
    {
        public string DataSource { get; set; }

        public string Command { get; set; }

        public OutputDefinition Clone()
        {
            OutputDefinition clone = new OutputDefinition()
            {
                DataSource = this.DataSource,
                Command = this.Command
            };
            return clone;
        }
    }
}