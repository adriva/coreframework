using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;

namespace Adriva.Extensions.Analytics.Server.AppInsights.Contracts
{
    public class Envelope
    {
        [JsonProperty("ver")]
        public int Version { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("Time")]
        public string Time { get; set; }

        [JsonProperty("sampleRate")]
        public double SampleRate { get; set; }

        [JsonProperty("seq")]
        public string Sequence { get; set; }

        [JsonProperty("iKey")]
        public string InstrumentationKey { get; set; }

        [JsonProperty("time")]
        public string Timestamp { get; set; }

        [JsonProperty("tags")]
        public Dictionary<string, string> Tags { get; set; }

        [JsonProperty("data")]
        public Base Data { get; set; }

        [JsonIgnore]
        public Domain EventData { get; internal set; }

        [JsonIgnore]
        public DateTime EventDate
        {
            get
            {
                if (DateTime.TryParse(this.Timestamp, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateTime))
                {
                    return dateTime;
                }
                else if (DateTime.TryParse(this.Timestamp, CultureInfo.CurrentCulture, DateTimeStyles.None, out dateTime))
                {
                    return dateTime;
                }
                else
                {
                    return DateTime.MinValue;
                }
            }
            set
            {
                this.Timestamp = value.ToString("yyyy-MM-dd HH:mm:ss.ffff");
            }
        }

        public Envelope()
            : this("AI.Envelope", "Envelope")
        { }

        protected Envelope(string fullName, string name)
        {
            this.Version = 1;
            this.Name = string.Empty;
            this.Time = string.Empty;
            this.SampleRate = 100.0;
            this.Sequence = string.Empty;
            this.InstrumentationKey = string.Empty;
            this.Tags = new Dictionary<string, string>();
            this.Data = new Base();
        }

        public T Unwrap<T>() where T : Domain
        {
            if (this.EventData is T instanceOfT) return instanceOfT;
            return null;
        }
    }
}