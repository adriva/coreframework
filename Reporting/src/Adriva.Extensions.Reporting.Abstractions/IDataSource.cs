using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        ICommand BuildCommand(CommandDefinition commandDefinition, FilterValuesCollection filterValues);
    }

    public interface IDataSource
    {
        ICommandBuilder GetCommandBuilder();
    }
}