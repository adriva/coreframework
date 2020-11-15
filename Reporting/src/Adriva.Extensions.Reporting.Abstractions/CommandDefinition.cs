using Adriva.Common.Core;
using Microsoft.Extensions.Configuration;

namespace Adriva.Extensions.Reporting.Abstractions
{
    public class CommandDefinition : IDynamicDefinition, ICloneable<CommandDefinition>
    {
        public string CommandText { get; set; }

        public IConfigurationSection Options { get; set; }

        public CommandDefinition Clone()
        {
            return new CommandDefinition()
            {
                CommandText = this.CommandText,
                Options = this.Options
            };
        }
    }
}