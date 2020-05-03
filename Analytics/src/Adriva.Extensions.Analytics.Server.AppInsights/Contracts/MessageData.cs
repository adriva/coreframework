using System.Collections.Generic;
using Newtonsoft.Json;

namespace Adriva.Extensions.Analytics.Server.AppInsights.Contracts
{
    public partial class MessageData
        : Domain
    {
        [JsonProperty("ver")]
        public int Version { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("severityLevel")]
        public SeverityLevel? SeverityLevel { get; set; }

        [JsonProperty("properties")]
        public Dictionary<string, string> Properties { get; set; }

        public MessageData()
            : this("AI.MessageData", "MessageData")
        { }

        protected MessageData(string fullName, string name)
        {
            this.Version = 2;
            this.Message = string.Empty;
            this.Properties = new Dictionary<string, string>();
        }
    }
}