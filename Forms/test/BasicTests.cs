using Adriva.Extensions.Forms;

namespace test;

[TestClass]
public sealed class BasicTests
{
    [TestMethod]
    public void SerializeBasicForm()
    {
        DynamicForm form = new("form-1");
        form.AddChild(new InputElement("input-1")
        {
            DisplayName = "Input 1",
            IsRequired = true,
            MaximumLength = 50
        });

        var choices1 = new ChoicesElement("select-1")
        {
            DisplayName = "Select 1",
            IsRequired = true,
        };

        choices1.Items["0"] = "Choice 1";
        choices1.Items["1"] = "Choice 2";

        form.AddChild(choices1);

        DynamicFormJsonSerializer serializer = new();
        string json = serializer.Serialize(form);

        var form2 = serializer.Deserialize(json);
    }
}
