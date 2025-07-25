using FASTER.core;
using Newtonsoft.Json;

namespace Adriva.Extensions.Faster
{
    public readonly struct FasterLockToken
    {
        [JsonProperty]
        public string Key { get; }

        [JsonProperty]
        public bool IsSuccess { get; }

        [JsonProperty]
        public string Value { get; }

        [JsonConstructor]
        internal FasterLockToken(bool isSuccess, string value, string key)
        {
            this.IsSuccess = isSuccess;
            this.Value = value;
            this.Key = key;
        }
    }
}