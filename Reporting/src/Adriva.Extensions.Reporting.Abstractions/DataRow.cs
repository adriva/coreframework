using System;

namespace Adriva.Extensions.Reporting.Abstractions
{
    public class DataRow
    {
        private readonly object[] Items;

        public DataRow(object[] items)
        {
            if (null == items || 0 == items.Length)
            {
                throw new ArgumentException("DataRow requires at least one data item");
            }
            this.Items = items;
        }
    }
}