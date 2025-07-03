using Adriva.Common.Core;
using Newtonsoft.Json;

namespace Adriva.Extensions.Forms;

public class DynamicFormJsonSerializer : IDynamicFormSerializer
{
    private static readonly JsonSerializerSettings DefaultJsonSerializerSettings;

    static DynamicFormJsonSerializer()
    {
        DynamicFormJsonSerializer.DefaultJsonSerializerSettings = new()
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
        };

        DynamicFormJsonSerializer.DefaultJsonSerializerSettings.Converters.Add(new DynamicFormItemJsonConverter());
    }

    protected virtual JsonSerializerSettings? GetSerializerSettings(bool isSerializing)
    {
        if (isSerializing)
        {
            var settings = new JsonSerializerSettings(DynamicFormJsonSerializer.DefaultJsonSerializerSettings)
            {
                ContractResolver = new DynamicFormItemContractResolver()
            };

            return settings;
        }

        return DynamicFormJsonSerializer.DefaultJsonSerializerSettings;
    }

    public string Serialize(DynamicForm dynamicForm)
    {
        return Utilities.SafeSerialize(dynamicForm, this.GetSerializerSettings(true));
    }

    public DynamicForm? Deserialize(string? data)
    {
        return Utilities.SafeDeserialize<DynamicForm>(data, this.GetSerializerSettings(false));
    }
}
