using Newtonsoft.Json;

namespace Adriva.Extensions.Forms;

[JsonObject(MemberSerialization.OptIn)]
public class ChoicesElement(string name) : DynamicFormElement(name)
{
    [JsonProperty("allowMultipleSelection")]
    public bool AllowMultipleSelection { get; set; }

    [JsonProperty("items")]
    public Dictionary<string, string> Items { get; } = [];
}
