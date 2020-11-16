using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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

    public sealed class ReportCommand
    {
        public IList<ReportCommandParameter> Parameters { get; private set; }

        public CommandDefinition CommandDefinition { get; private set; }

        public string Text { get; private set; }

        public ReportCommand(string text, CommandDefinition commandDefinition)
        {
            this.Text = text;
            this.CommandDefinition = commandDefinition;
            this.Parameters = new List<ReportCommandParameter>();
        }
    }

    public sealed class ReportCommandParameter
    {
        public string Name { get; private set; }

        public FilterValue FilterValue { get; private set; }

        public ReportCommandParameter(string name, FilterValue filterValue)
        {
            this.Name = name;
            this.FilterValue = filterValue;
        }
    }

    public interface IDataSource { }
}