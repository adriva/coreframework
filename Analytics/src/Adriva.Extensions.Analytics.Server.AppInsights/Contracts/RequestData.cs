using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Adriva.Extensions.Analytics.Server.AppInsights.Contracts
{
    public partial class RequestData
        : EventData
    {

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("source")]
        public string Source { get; set; }

        [JsonProperty("duration")]
        public string Duration { get; set; }

        [JsonProperty("responseCode")]
        public string ResponseCode { get; set; }

        [JsonProperty("success")]
        public bool IsSuccess { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }


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

        public RequestData()
                    : this("AI.RequestData", "RequestData")
        { }

        protected RequestData(string fullName, string name)
        {
            this.Version = 2;
            this.Id = string.Empty;
            this.Source = string.Empty;
            this.Name = string.Empty;
            this.Duration = string.Empty;
            this.ResponseCode = string.Empty;
            this.Url = string.Empty;
            this.Properties = new Dictionary<string, string>();
            this.Measurements = new Dictionary<string, double>();
        }
    }
}