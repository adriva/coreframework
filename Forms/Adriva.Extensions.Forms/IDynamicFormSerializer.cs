namespace Adriva.Extensions.Forms;

public interface IDynamicFormSerializer
{
    string? Serialize(DynamicForm dynamicForm);

    DynamicForm? Deserialize(string? data);
}
