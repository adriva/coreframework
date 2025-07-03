using Newtonsoft.Json;

namespace Adriva.Extensions.Forms;

[JsonObject(MemberSerialization.OptIn)]
public class InputElement(string name) : DynamicFormElement(name)
{
    [JsonProperty("minLength")]
    public int MinimumLength { get; set; }

    [JsonProperty("maxLength")]
    public int MaximumLength { get; set; }
}
