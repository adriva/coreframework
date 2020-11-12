using Microsoft.Extensions.Configuration;

namespace Adriva.Extensions.Reporting.Abstractions
{
    public interface IDynamicDefinition
    {
        IConfigurationSection Options { get; set; }
    }
}