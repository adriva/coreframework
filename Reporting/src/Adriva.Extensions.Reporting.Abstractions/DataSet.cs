using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Adriva.Extensions.Reporting.Abstractions
{
    public class DataSet
    {
        private readonly IList<DataColumn> DataColumns = new List<DataColumn>(8);
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

        private DataSet() { }

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