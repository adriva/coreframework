using System.Collections.ObjectModel;
using Newtonsoft.Json;

namespace Adriva.Extensions.Forms;

[JsonObject(MemberSerialization.OptIn)]
public class DynamicFormItemContainer(string name) : DynamicFormItem(name)
{
    [JsonProperty(PropertyName = "children")]
    protected readonly List<DynamicFormItem> ChildItems = [];

    [JsonIgnore]
    public ReadOnlyCollection<DynamicFormItem> Children => new(this.ChildItems);

    public virtual void AddChild(DynamicFormItem child)
    {
        this.ChildItems.Add(child);
    }
}
