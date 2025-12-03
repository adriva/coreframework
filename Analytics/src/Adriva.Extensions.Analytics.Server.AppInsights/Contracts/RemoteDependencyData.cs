using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Adriva.Extensions.Analytics.Server.AppInsights.Contracts
{
    public partial class RemoteDependencyData
        : Domain
    {
        [JsonProperty("ver")]
        public int Version { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("resultCode")]
        public string ResultCode { get; set; }

        [JsonProperty("duration")]
        public string Duration { get; set; }

        [JsonProperty("success")]
        public bool? IsSuccess { get; set; }

        [JsonProperty("data")]
        public string Data { get; set; }

        [JsonProperty("target")]
        public string Target { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("properties")]
        public Dictionary<string, string> Properties { get; set; }

        [JsonProperty("measurements")]
        public Dictionary<string, double> Measurements { get; set; }

        [JsonIgnore]
        public double DurationInMilliseconds
        {
            get
            {
                if (TimeSpan.TryParse(this.Duration, out TimeSpan timespan))
                {
                    return timespan.TotalMilliseconds;
                }
                return -1;
            }
            set
            {
                this.Duration = TimeSpan.FromMilliseconds(value).ToString();
            }
        }

        public RemoteDependencyData()
            : this("AI.RemoteDependencyData", "RemoteDependencyData")
        { }

        protected RemoteDependencyData(string fullName, string name)
        {
            this.Version = 2;
            this.Name = string.Empty;
            this.Id = string.Empty;
            this.ResultCode = string.Empty;
            this.Duration = string.Empty;
            this.IsSuccess = true;
            this.Data = string.Empty;
            this.Target = string.Empty;
            this.Type = string.Empty;
            this.Properties = new Dictionary<string, string>();
            this.Measurements = new Dictionary<string, double>();
        }
    }
}