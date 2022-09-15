using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Newtonsoft.Json;

namespace Adriva.Extensions.Reporting.Abstractions
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class DataSet
    {
        [JsonProperty("dataColumns")]
        private readonly IList<DataColumn> DataColumns = new List<DataColumn>(8);

        [JsonProperty("dataRows")]
        private readonly IList<DataRow> DataRows = new List<DataRow>(64);

        public ReadOnlyCollection<DataColumn> Columns => new ReadOnlyCollection<DataColumn>(this.DataColumns);

        public ReadOnlyCollection<DataRow> Rows => new ReadOnlyCollection<DataRow>(this.DataRows);

        public static DataSet FromFields(FieldDefinition[] fields)
        {
            DataSet dataSet = new DataSet();
            foreach (var field in fields)
            {
                dataSet.DataColumns.Add(new DataColumn(field.Name, field.DisplayName));
            }
            return dataSet;
        }

        public static DataSet FromColumnNames(params string[] names)
        {
            if (null == names || 0 == names.Length)
            {
                throw new ArgumentException("Data set requires at least one field with a non-empty name to construct its columns.", nameof(names));
            }

            return DataSet.FromFields(names.Select(n => new FieldDefinition() { Name = n }).ToArray());
        }

        public IDictionary<string, object> Metadata { get; }

        [JsonConstructor]
        private DataSet()
        {
            this.Metadata = new Dictionary<string, object>();
        }

        public DataRow CreateRow()
        {
            if (0 == this.DataColumns.Count)
            {
                throw new InvalidOperationException("There is no fields defined in the dataset.");
            }
            DataRow dataRow = new DataRow(this);
            this.DataRows.Add(dataRow);
            return dataRow;
        }
    }
}