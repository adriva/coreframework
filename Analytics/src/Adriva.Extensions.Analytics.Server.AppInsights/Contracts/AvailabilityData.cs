using System.Collections.Generic;
using Newtonsoft.Json;

namespace Adriva.Extensions.Analytics.Server.AppInsights.Contracts
{
    public partial class AvailabilityData
            : Domain
    {
        [JsonProperty("ver")]
        public int Version { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("duration")]
        public string Duration { get; set; }

        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("runLocation")]
        public string RunLocation { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("properties")]
        public Dictionary<string, string> Properties { get; set; }

        [JsonProperty("Measurements")]
        public Dictionary<string, double> Measurements { get; set; }

        public AvailabilityData()
            : this("AI.AvailabilityData", "AvailabilityData")
        { }

        protected AvailabilityData(string fullName, string name)
        {
            this.Version = 2;
            this.Id = string.Empty;
            this.Name = string.Empty;
            this.Duration = string.Empty;
            this.RunLocation = string.Empty;
            this.Message = string.Empty;
            this.Properties = new Dictionary<string, string>();
            this.Measurements = new Dictionary<string, double>();
        }
    }
}