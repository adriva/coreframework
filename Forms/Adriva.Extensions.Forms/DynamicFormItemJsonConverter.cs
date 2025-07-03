using Adriva.Common.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Adriva.Extensions.Forms;

public class DynamicFormItemJsonConverter : JsonConverter<DynamicFormItem>
{
    public const string DiscriminatorPropertyName = "kind";

    private static readonly Type TypeOfDynamicFormItem = typeof(DynamicFormItem);

    public override bool CanWrite => false;

    public override bool CanRead => true;

    protected virtual DynamicFormItem? CreateItemInstance(JsonObjectContract contract, JObject jobject)
    {
        if (contract.DefaultCreator is not null)
        {
            return (DynamicFormItem?)contract.DefaultCreator();
        }

        List<object?> ctorParameters = [];

        if (contract.CreatorParameters is not null)
        {
            foreach (var creatorParameter in contract.CreatorParameters)
            {
                if (creatorParameter is not null && !string.IsNullOrWhiteSpace(creatorParameter.PropertyName))
                {
                    ctorParameters.Add(
                        jobject
                            .Property(creatorParameter.PropertyName, StringComparison.OrdinalIgnoreCase)?
                            .ToObject(creatorParameter.PropertyType ?? typeof(object)));
                }
            }
        }

        return (DynamicFormItem?)Activator.CreateInstance(contract.CreatedType, [.. ctorParameters]);
    }

    protected virtual Type[] ResolveMatchingTypes(string kind)
    {
        if (string.IsNullOrWhiteSpace(kind))
        {
            return [];
        }

        return [..ReflectionHelpers
                    .FindTypes(type => type.IsClass && !type.IsSpecialName && !type.IsAbstract && !type.IsGenericType && DynamicFormItemJsonConverter.TypeOfDynamicFormItem.IsAssignableFrom(type) && kind.Equals(type.Name, StringComparison.OrdinalIgnoreCase))
                    .Take(2)];
    }

    public override DynamicFormItem? ReadJson(JsonReader reader, Type objectType, DynamicFormItem? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        if (JsonToken.Null == reader.TokenType)
        {
            return null;
        }

        JToken token = JToken.Load(reader);

        if (token is JObject itemObject)
        {
            JProperty? kindProperty = itemObject.Property(DynamicFormItemJsonConverter.DiscriminatorPropertyName, StringComparison.OrdinalIgnoreCase);

            if (kindProperty is not null && kindProperty.Value is JValue kindValue && JTokenType.String == kindValue.Type)
            {
                string? targetKind = kindValue.Value<string>();

                if (!string.IsNullOrWhiteSpace(targetKind))
                {
                    Type[] matchingTypes = this.ResolveMatchingTypes(targetKind);

                    if (0 == matchingTypes.Length)
                    {
                        throw new JsonSerializationException($"Could not find the '{DynamicFormItemJsonConverter.DiscriminatorPropertyName}' name in JSON. {DynamicFormItemJsonConverter.DiscriminatorPropertyName} field is required to resolve the target form item type.");
                    }
                    else if (2 == matchingTypes.Length)
                    {
                        throw new InvalidOperationException($"Element kind specified '{targetKind}' matches multiple targets. {string.Join(',', matchingTypes.Select(x => x.FullName))}");
                    }

                    if (existingValue is null || existingValue.GetType() != matchingTypes[0])
                    {
                        var contract = (JsonObjectContract)serializer.ContractResolver.ResolveContract(matchingTypes[0]);

                        existingValue = this.CreateItemInstance(contract, itemObject);

                        using JsonReader itemReader = token.CreateReader();

                        if (existingValue is not null)
                        {
                            serializer.Populate(itemReader, existingValue);
                            return existingValue;
                        }
                    }
                }
            }
        }

        throw new JsonSerializationException();
    }

    public override void WriteJson(JsonWriter writer, DynamicFormItem? value, JsonSerializer serializer)
    {

    }
}
