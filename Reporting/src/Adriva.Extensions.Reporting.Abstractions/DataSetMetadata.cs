using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Adriva.Extensions.Reporting.Abstractions
{
    [JsonDictionary()]
    public sealed class DataSetMetadata : Dictionary<string, object>
    {
        public long? RecordCount
        {
            get => this.GetValue<long>(nameof(DataSetMetadata.RecordCount));
            set => this[nameof(DataSetMetadata.RecordCount)] = value;
        }

        public int? PageNumber
        {
            get => this.GetValue<int>(nameof(DataSetMetadata.PageNumber));
            set => this[nameof(DataSetMetadata.PageNumber)] = value;
        }

        public int? PageCount
        {
            get => this.GetValue<int>(nameof(DataSetMetadata.PageCount));
            set => this[nameof(DataSetMetadata.PageCount)] = value;
        }

        public IDictionary<string, object> Items => this.ToDictionary(x => x.Key, x => x.Value);

        [JsonConstructor]
        public DataSetMetadata() : base(StringComparer.OrdinalIgnoreCase)
        {

        }

        private T? GetValue<T>(string key) where T : struct
        {
            if (!this.TryGetValue(key, out object value))
            {
                return null;
            }

            return (T)value;
        }
    }
}