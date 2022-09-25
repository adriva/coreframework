using System;
using System.Linq;
using Adriva.Extensions.Reporting.Abstractions;
using Microsoft.Extensions.FileProviders;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Adriva.DevTools.Cli.Reporting
{
    internal sealed class ReportDefinitionConverter : JsonConverter<ReportDefinition>
    {
        private readonly string SchemaPath;

        public override bool CanRead => false;

        public override bool CanWrite => true;

        public ReportDefinitionConverter(string schemaPath)
        {
            this.SchemaPath = schemaPath;
        }

        public override ReportDefinition ReadJson(JsonReader reader, Type objectType, ReportDefinition existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, ReportDefinition value, JsonSerializer serializer)
        {
            int index = serializer.Converters.IndexOf(this);

            if (-1 < index)
            {
                serializer.Converters.RemoveAt(index);
            }

            var jobject = JObject.FromObject(value, serializer);
            jobject.AddFirst(new JProperty("$schema", this.SchemaPath));

            jobject.WriteTo(writer, serializer.Converters.ToArray());

            if (-1 < index)
            {
                serializer.Converters.Insert(index, this);
            }
        }
    }
}