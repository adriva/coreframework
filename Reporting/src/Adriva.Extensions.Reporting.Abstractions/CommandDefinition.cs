using Adriva.Common.Core;
using Newtonsoft.Json.Linq;

namespace Adriva.Extensions.Reporting.Abstractions
{
    public sealed class CommandDefinition : IDynamicDefinition, ICloneable<CommandDefinition>
    {
        public string CommandText { get; set; }

        public JToken Options { get; set; }

        public CommandDefinition Clone()
        {
            return new CommandDefinition()
            {
                CommandText = this.CommandText,
                Options = this.Options?.DeepClone()
            };
        }
    }
}