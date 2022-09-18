using System;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace Adriva.Extensions.Reporting.Abstractions
{
    [DebuggerDisplay("{DebugView}")]
    public class DataColumn
    {
        public string Name { get; private set; }

        public string DisplayName { get; private set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public TypeCode DataType { get; private set; }

        public string Format { get; private set; }

        public JToken Options { get; private set; }

        private string DebugView
        {
            get
            {
                return $"{this.Name} ({this.DisplayName ?? "NULL"})";
            }
        }

        public DataColumn(string name, TypeCode dataType = TypeCode.String, string displayName = null, string format = null, JToken options = null)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException("DataColumn requires a name.");

            this.Name = name;
            this.DisplayName = displayName;
            this.DataType = dataType;
            this.Format = format;
            this.Options = options;
        }
    }
}