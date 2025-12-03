using Newtonsoft.Json.Serialization;

namespace Adriva.Extensions.Forms;

public class DynamicFormItemContractResolver : DefaultContractResolver
{
    protected override JsonObjectContract CreateObjectContract(Type objectType)
    {
        JsonObjectContract contract = base.CreateObjectContract(objectType);

        contract.Properties.Add(new JsonProperty()
        {
            PropertyType = typeof(string),
            PropertyName = DynamicFormItemJsonConverter.DiscriminatorPropertyName
        });

        contract.ExtensionDataGetter = (object item) =>
        {
            if (item is DynamicFormItem dynamicFormItem)
            {
                return [
                    new(DynamicFormItemJsonConverter.DiscriminatorPropertyName, dynamicFormItem.GetType().Name)
                ];
            }

            return [];
        };

        return contract;
    }
}