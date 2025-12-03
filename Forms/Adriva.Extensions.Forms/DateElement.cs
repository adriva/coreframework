using Newtonsoft.Json;

namespace Adriva.Extensions.Forms;

[JsonObject(MemberSerialization.OptIn)]
public class DateElement(string name) : DynamicFormElement(name)
{
    [JsonProperty("minDate")]
    public DateTime? MinimumLength { get; set; }

    [JsonProperty("maxDate")]
    public DateTime? MaximumDate { get; set; }
}
