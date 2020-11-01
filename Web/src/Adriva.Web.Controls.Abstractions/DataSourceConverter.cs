using System;
using Adriva.Common.Core;
using Newtonsoft.Json;

namespace Adriva.Web.Controls.Abstractions
{
    public sealed class DataSourceConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) => objectType == typeof(object);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            string json = Utilities.SafeSerialize(value);
            writer.WriteRawValue(json);
        }
    }
}