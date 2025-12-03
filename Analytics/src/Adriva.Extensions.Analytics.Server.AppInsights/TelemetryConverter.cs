using System.Linq;
using System;
using Newtonsoft.Json;
using Adriva.Extensions.Analytics.Server.AppInsights.Contracts;
using Newtonsoft.Json.Linq;

namespace Adriva.Extensions.Analytics.Server.AppInsights
{
    public class TelemetryConverter : JsonConverter
    {
        private static readonly Type TypeOfTelemetry = typeof(Envelope);

        private static readonly Type[] KnownTypes = new[] {
            typeof(Contracts.AvailabilityData),
            typeof(Contracts.EventData),
            typeof(Contracts.ExceptionData),
            typeof(Contracts.MessageData),
            typeof(Contracts.MetricData),
            typeof(Contracts.PageViewData),
            typeof(Contracts.PageViewPerfData),
            typeof(Contracts.RemoteDependencyData),
            typeof(Contracts.RequestData),
        };

        public override bool CanConvert(Type objectType)
        {
            return TelemetryConverter.TypeOfTelemetry.IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jObject = serializer.Deserialize<JObject>(reader);

            var envelope = jObject.ToObject<Contracts.Envelope>();
            var typeName = envelope.Data.BaseType;

            var matchinTelemetryType = TelemetryConverter.KnownTypes.FirstOrDefault(kt => 0 == string.Compare(kt.Name, typeName, StringComparison.OrdinalIgnoreCase));

            if (null != matchinTelemetryType)
            {
                JProperty dataProperty = jObject.Property("data");
                if (null != dataProperty && dataProperty.Value is JObject propertyObject)
                {
                    JProperty baseDataProperty = propertyObject.Property("baseData");
                    if (null != baseDataProperty?.Value)
                    {
                        try
                        {
                            envelope.EventData = (Domain)baseDataProperty.Value.ToObject(matchinTelemetryType);
                            return envelope;
                        }
                        catch
                        {
                            //?
                            return null;
                        }
                    }
                }

            }
            return null;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}