using Newtonsoft.Json;

namespace Adriva.Extensions.Faster
{

    public readonly struct CacheLockRequest
    {
        [JsonProperty]
        public string Key { get; }

        [JsonProperty]
        public int? TimeToLive { get; } // in milliseconds

        [JsonConstructor]
        public CacheLockRequest(string key, int? timeToLive)
        {
            this.Key = key;
            this.TimeToLive = timeToLive;
        }
    }

    public struct CacheUpsertRequest
    {
        [JsonProperty]
        public string Key { get; private set; }

        [JsonProperty]
        public object Value { get; private set; }

        [JsonIgnore]
        internal string ETag { get; set; }

        [JsonProperty]
        public int? TimeToLive { get; } // in milliseconds

        public CacheUpsertRequest(string key, object value)
            : this(key, value, null)
        {

        }

        [JsonConstructor]
        public CacheUpsertRequest(string key, object value, int? timeToLive)
        {
            this.Key = key;
            this.Value = value;
            this.TimeToLive = timeToLive;
        }
    }
}