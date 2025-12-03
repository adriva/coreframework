using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Adriva.Extensions.Forms;

[DebuggerDisplay("{GetType().Name} ({Name})")]
[JsonObject(MemberSerialization.OptIn)]
public abstract class DynamicFormItem(string name)
{
    [JsonProperty("name")]
    public string Name { get; } = name;

    [JsonProperty("displayName")]
    public string? DisplayName { get; set; }

    [JsonProperty("metadata")]
    public JObject? Metadata { get; set; }
}
