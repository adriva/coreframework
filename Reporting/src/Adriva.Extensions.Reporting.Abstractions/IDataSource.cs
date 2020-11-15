using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

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

    public class DataColumn
    {
        public string Name { get; private set; }

        public TypeCode DataType { get; private set; }

        public DataColumn(string name, TypeCode dataType = TypeCode.Object)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException("DataColumn requires a name.");
            if (TypeCode.Empty == dataType) throw new ArgumentException("DataType of a DataColumn cannot be TypeCode.Empty.");

            this.Name = name;
            this.DataType = dataType;
        }
    }

    public class DataSet
    {
        private readonly IList<DataColumn> DataColumns = new List<DataColumn>(8);
        private readonly IList<DataRow> DataRows = new List<DataRow>(64);

        public IEnumerable<DataColumn> Columns => this.DataColumns.AsEnumerable();
        public IEnumerable<DataRow> Rows => this.DataRows.AsEnumerable();


    }

    public interface ICommand
    {

    }

    public interface ICommandParameter
    {

    }

    public interface ICommandBuilder
    {
        ICommand BuildCommand(ReportCommandContext context, FilterValues filterValues);
    }

    public interface IDataSource { }

    public sealed class FilterValue
    {
        public object RawValue { get; private set; }

        public object Value { get; private set; }
    }

    public sealed class FilterValues
    {
        public FilterValue this[string name] => null;
    }

    public interface IFilterValueProvider
    {
        FilterValues Resolve(ReportCommandContext reportCommandContext, IDictionary<string, string> values);
    }

}