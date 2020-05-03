using System;
using Newtonsoft.Json;

namespace Adriva.Extensions.Analytics.Server.AppInsights.Contracts
{
    public partial class PageViewData
        : EventData
    {
        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("duration")]
        public string Duration { get; set; }

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

        public PageViewData()
            : this("AI.PageViewData", "PageViewData")
        { }

        protected PageViewData(string fullName, string name)
        {
            this.Url = string.Empty;
            this.Duration = string.Empty;
        }
    }
}