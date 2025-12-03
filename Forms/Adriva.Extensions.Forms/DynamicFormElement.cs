using Newtonsoft.Json;

namespace Adriva.Extensions.Forms;

[JsonObject(MemberSerialization.OptIn)]
public abstract class DynamicFormElement(string name) : DynamicFormItem(name)
{
    [JsonProperty("value")]
    public object? Value { get; set; }

    [JsonProperty("isRequired")]
    public bool IsRequired { get; set; }

    [JsonProperty("isDisabled")]
    public bool IsDisabled { get; set; }
}
