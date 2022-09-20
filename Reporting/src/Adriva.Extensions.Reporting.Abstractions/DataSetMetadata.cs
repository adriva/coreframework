using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Adriva.Extensions.Reporting.Abstractions
{
    public sealed class DataSetMetadata : Dictionary<string, object>
    {
        public long RecordCount { get; set; }

        [JsonConstructor]
        public DataSetMetadata() : base(StringComparer.OrdinalIgnoreCase)
        {

        }
    }
}