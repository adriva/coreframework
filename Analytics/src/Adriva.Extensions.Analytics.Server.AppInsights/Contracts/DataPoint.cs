using Adriva.Extensions.Analytics.Server.Entities;
using Newtonsoft.Json;

namespace Adriva.Extensions.Analytics.Server.AppInsights.Contracts
{
    public partial class DataPoint
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("kind")]
        public DataPointType Kind { get; set; }

        [JsonProperty("value")]
        public double Value { get; set; }

        [JsonProperty("count")]
        public int? Count { get; set; }

        [JsonProperty("min")]
        public double? Minimum { get; set; }

        [JsonProperty("max")]
        public double? Maximum { get; set; }

        [JsonProperty("stdDev")]
        public double? StandardDeviation { get; set; }

        public DataPoint()
            : this("AI.DataPoint", "DataPoint")
        { }

        protected DataPoint(string fullName, string name)
        {
            this.Name = string.Empty;
            this.Kind = DataPointType.Measurement;
        }
    }
}