using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Adriva.Common.Core;
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
                dataSet.DataColumns.Add(new DataColumn(field.Name, field.DataType, field.DisplayName, field.Format, field.Options));
            }
            return dataSet;
        }

        public static DataSet FromColumnNames(params string[] names)
        {
            if (null == names || 0 == names.Length)
            {
                throw new ArgumentException("Dataset requires at least one field with a non-empty name to construct its columns.", nameof(names));
            }

            return DataSet.FromFields(names.Select(n => new FieldDefinition() { Name = n }).ToArray());
        }

        [JsonProperty("metadata")]
        public DataSetMetadata Metadata { get; } = new DataSetMetadata();

        [JsonConstructor]
        private DataSet()
        {

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

        public void AddColumn(DataColumn dataColumn)
        {
            if (null == dataColumn)
            {
                throw new ArgumentNullException(nameof(dataColumn));
            }

            if (null == dataColumn.Name)
            {
                throw new ArgumentNullException("Data column must have a name. Null names are not supported.");
            }

            if (this.DataColumns.Any(c => 0 == string.Compare(c.Name, dataColumn.Name)))
            {
                throw new InvalidOperationException($"Dataset already has a column named '{dataColumn.Name}'.");
            }

            this.DataColumns.Add(dataColumn);
        }

        public void SpliceRows(int startIndex, int length)
        {
            if (0 > startIndex || 0 >= length)
            {
                return;
            }
            else if (0 == this.DataRows.Count)
            {
                return;
            }

            Queue<DataRow> deleteQueue = new Queue<DataRow>();
            this.DataRows.Skip(startIndex).Take(length).ForEach((index, row) =>
            {
                deleteQueue.Enqueue(row);
            });

            while (deleteQueue.TryDequeue(out DataRow deleteRow))
            {
                this.DataRows.Remove(deleteRow);
            }
        }

        public DataSet CopyStructure()
        {
            var clone = new DataSet();

            this.DataColumns.ForEach((index, column) =>
            {
                clone.DataColumns.Add(column);
            });

            return clone;
        }
    }
}