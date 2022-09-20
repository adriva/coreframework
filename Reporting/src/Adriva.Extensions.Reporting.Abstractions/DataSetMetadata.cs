using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Adriva.Extensions.Reporting.Abstractions
{
    public sealed class DataSetMetadata : Dictionary<string, object>
    {
        [JsonProperty()]
        public long? RecordCount { get; set; }

        [JsonProperty]
        public int? PageNumber { get; set; }

        [JsonProperty]
        public int? PageCount { get; set; }

        [JsonConstructor]
        public DataSetMetadata() : base(StringComparer.OrdinalIgnoreCase)
        {

        }
    }
}