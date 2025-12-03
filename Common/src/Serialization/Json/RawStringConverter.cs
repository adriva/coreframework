using System;
using Newtonsoft.Json;

namespace Adriva.Common.Core.Serialization.Json
{
    public sealed class RawStringConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(RawString) == objectType;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return reader.Value;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteRawValue((RawString)value);
        }
    }
}