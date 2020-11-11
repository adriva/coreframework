using Microsoft.Extensions.Configuration;

namespace Adriva.Extensions.Reporting.Abstractions
{
    public abstract class DynamicDefinition
    {
        public IConfigurationSection Options { get; set; }
    }
}