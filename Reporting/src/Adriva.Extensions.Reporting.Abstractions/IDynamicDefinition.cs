using Newtonsoft.Json.Linq;

namespace Adriva.Extensions.Reporting.Abstractions
{
    public interface IDynamicDefinition
    {
        JToken Options { get; set; }
    }
}