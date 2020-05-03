using System.Collections.Generic;
using Newtonsoft.Json;

namespace Adriva.Extensions.Analytics.Server.AppInsights.Contracts
{
    public partial class ExceptionData
        : Domain
    {
        [JsonProperty("ver")]
        public int Version { get; set; }

        [JsonProperty("exceptions")]
        public List<ExceptionDetails> Exceptions { get; set; }

        [JsonProperty("severityLevel")]
        public SeverityLevel? SeverityLevel { get; set; }

        [JsonProperty("problemId")]
        public string ProblemId { get; set; }

        [JsonProperty("properties")]
        public Dictionary<string, string> Properties { get; set; }

        [JsonProperty("measurements")]
        public Dictionary<string, double> Measurements { get; set; }

        public ExceptionData()
            : this("AI.ExceptionData", "ExceptionData")
        { }

        protected ExceptionData(string fullName, string name)
        {
            this.Version = 2;
            this.Exceptions = new List<ExceptionDetails>();
            this.ProblemId = string.Empty;
            this.Properties = new Dictionary<string, string>();
            this.Measurements = new Dictionary<string, double>();
        }
    }
}