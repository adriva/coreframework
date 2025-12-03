using Newtonsoft.Json;

namespace Adriva.Extensions.Forms;

[JsonObject(MemberSerialization.OptIn)]
public class DynamicForm(string name) : DynamicFormItemContainer(name)
{

}
