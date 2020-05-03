using System.Collections.Generic;
using Newtonsoft.Json;

namespace Adriva.Extensions.Analytics.Server.AppInsights.Contracts
{
    public partial class EventData
            : Domain
    {
        [JsonProperty("ver")]
        public int Version { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("properties")]
        public Dictionary<string, string> Properties { get; set; }

        [JsonProperty("measurements")]
        public Dictionary<string, double> Measurements { get; set; }

        public EventData()
            : this("AI.EventData", "EventData")
        { }

        protected EventData(string fullName, string name)
        {
            this.Version = 2;
            this.Name = string.Empty;
            this.Properties = new Dictionary<string, string>();
            this.Measurements = new Dictionary<string, double>();
        }
    }
}