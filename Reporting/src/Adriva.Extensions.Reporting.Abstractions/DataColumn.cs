using System;

namespace Adriva.Extensions.Reporting.Abstractions
{
    public class DataColumn
    {
        public string Name { get; private set; }

        public string DisplayName { get; private set; }

        public DataColumn(string name, string displayName = null)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException("DataColumn requires a name.");

            this.Name = name;
            this.DisplayName = string.IsNullOrWhiteSpace(displayName) ? name : displayName;

        }
    }
}