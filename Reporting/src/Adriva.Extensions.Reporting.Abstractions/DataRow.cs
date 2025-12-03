using System;
using System.Diagnostics;
using Adriva.Common.Core;

namespace Adriva.Extensions.Reporting.Abstractions
{
    [DebuggerDisplay("{DebugView}")]
    public class DataRow : DynamicItem
    {
        private readonly DataSet DataSet = null;
        private object[] Items = null;
        private int ItemPointer = 0;

        private string DebugView
        {
            get
            {
                return $"[{string.Join(",", this.Items)}]";
            }
        }

        internal DataRow(DataSet dataSet)
        {
            this.DataSet = dataSet;
            this.Items = new object[dataSet.Columns.Count];
        }

        public void AddData(object value)
        {
            if (this.ItemPointer == this.Items.Length)
            {
                throw new IndexOutOfRangeException("DataRow does not accept adding more data values than configured number of fields.");
            }
            this.Items[this.ItemPointer] = value;
            this[this.DataSet.Columns[this.ItemPointer].Name] = value;
            ++this.ItemPointer;
        }
    }
}