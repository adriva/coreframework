using Microsoft.Extensions.Configuration;

namespace Adriva.Extensions.Reporting.Abstractions
{
    public class CommandDefinition : IDynamicDefinition
    {
        public string CommandText { get; set; }

        public IConfigurationSection Options { get; set; }
    }
}