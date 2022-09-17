using System;
using System.Diagnostics;
using Newtonsoft.Json.Linq;

namespace Adriva.Extensions.Reporting.Abstractions
{
    [DebuggerDisplay("{DebugView}")]
    public class DataColumn
    {
        public string Name { get; private set; }

        public string DisplayName { get; private set; }

        public JToken Options { get; private set; }

        private string DebugView
        {
            get
            {
                return $"{this.Name} ({this.DisplayName ?? "NULL"})";
            }
        }

        public DataColumn(string name, string displayName = null, JToken options = null)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException("DataColumn requires a name.");

            this.Name = name;
            this.DisplayName = string.IsNullOrWhiteSpace(displayName) ? name : displayName;
            this.Options = options;
        }
    }
}