using System.Collections.Generic;
using Newtonsoft.Json;

namespace Adriva.Extensions.Analytics.Server.AppInsights.Contracts
{
    public partial class MetricData
        : Domain
    {
        [JsonProperty("ver")]
        public int Version { get; set; }

        [JsonProperty("metrics")]
        public List<DataPoint> Metrics { get; set; }

        [JsonProperty("properties")]
        public Dictionary<string, string> Properties { get; set; }

        public MetricData()
            : this("AI.MetricData", "MetricData")
        { }

        protected MetricData(string fullName, string name)
        {
            this.Version = 2;
            this.Metrics = new List<DataPoint>();
            this.Properties = new Dictionary<string, string>();
        }
    }
}