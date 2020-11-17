using System;
using Adriva.Common.Core;

namespace Adriva.Extensions.Reporting.Abstractions
{
    public class DataRow : DynamicItem
    {
        private readonly DataSet DataSet = null;
        private object[] Items = null;
        private int ItemPointer = 0;

        internal DataRow(DataSet dataSet)
        {
            this.DataSet = dataSet;
            this.Items = new object[dataSet.Columns.Count];
        }

        public void AddData(object value)
        {
            if (this.ItemPointer == this.Items.Length)
            {
                throw new IndexOutOfRangeException("DataRow does not accept adding more than configured fields.");
            }
            this.Items[this.ItemPointer] = value;
            this[this.DataSet.Columns[this.ItemPointer].Name] = value;
            ++this.ItemPointer;
        }
    }
}