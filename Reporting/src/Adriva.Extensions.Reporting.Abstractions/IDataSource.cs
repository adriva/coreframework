using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
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

    public sealed class FilterValue
    {
        public object RawValue { get; private set; }

        public object Value { get; private set; }
    }

    public sealed class FilterDefinitionDictionary : IDictionary<string, FilterDefinition>
    {
        private readonly IDictionary<string, FilterDefinition> Definitions;

        public FilterDefinitionDictionary()
        {
            this.Definitions = new Dictionary<string, FilterDefinition>(new FilterNameComparer());
        }

        public FilterDefinition this[string key] { get => Definitions[key]; set => Definitions[key] = value; }

        public ICollection<string> Keys => Definitions.Keys;

        public ICollection<FilterDefinition> Values => Definitions.Values;

        public int Count => Definitions.Count;

        public bool IsReadOnly => Definitions.IsReadOnly;

        public void Add(string key, FilterDefinition value)
        {
            Definitions.Add(key, value);
        }

        public void Add(KeyValuePair<string, FilterDefinition> item)
        {
            Definitions.Add(item);
        }

        public void Clear()
        {
            Definitions.Clear();
        }

        public bool Contains(KeyValuePair<string, FilterDefinition> item)
        {
            return Definitions.Contains(item);
        }

        public bool ContainsKey(string key)
        {
            return Definitions.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<string, FilterDefinition>[] array, int arrayIndex)
        {
            Definitions.CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<string, FilterDefinition>> GetEnumerator()
        {
            return Definitions.GetEnumerator();
        }

        public bool Remove(string key)
        {
            return Definitions.Remove(key);
        }

        public bool Remove(KeyValuePair<string, FilterDefinition> item)
        {
            return Definitions.Remove(item);
        }

        public bool TryGetValue(string key, out FilterDefinition value)
        {
            return Definitions.TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)Definitions).GetEnumerator();
        }
    }

    public interface ICommandBuilder
    {
        ReportCommand BuildCommand(ReportCommandContext context, IDictionary<string, string> values);
    }

    public interface IFilterValueBinder
    {
        FilterValue GetFilterValue(FilterDefinition filterDefinition, string rawValue);
    }

    public class DefaultFilterValueBinder : IFilterValueBinder
    {
        public FilterValue GetFilterValue(FilterDefinition filterDefinition, string rawValue)
        {
            throw new NotImplementedException();
        }
    }

    public class DefaultCommandBuilder : ICommandBuilder
    {
        protected IFilterValueBinder FilterValueBinder { get; private set; }

        public DefaultCommandBuilder(IFilterValueBinder filterValueBinder)
        {
            this.FilterValueBinder = filterValueBinder;
        }

        protected virtual bool TryParseCommandText(string text, out string commandText, out IList<string> parameterNames)
        {
            commandText = null;
            parameterNames = null;

            var matches = Regex.Matches(text, @"(?<parameterName>\@\w+)");

            if (null == matches) return false;

            parameterNames = new List<string>();

            foreach (Match match in matches)
            {
                if (match.Success)
                {
                    var parameterNameGroup = match.Groups["parameterName"];

                    if (parameterNameGroup.Success)
                    {
                        parameterNames.Add(parameterNameGroup.Value);
                    }
                }
            }

            commandText = text;

            return !string.IsNullOrWhiteSpace(commandText);
        }

        public ReportCommand BuildCommand(ReportCommandContext context, IDictionary<string, string> values)
        {
            if (!this.TryParseCommandText(context.CommandDefinition.CommandText, out string commandText, out IList<string> parameterNames))
            {
                throw new InvalidOperationException($"Failed to parse command '{context.CommandName}' in report '{context.ReportDefinition.Name}'.");
            }

            ReportCommand command = new ReportCommand(commandText, context.CommandDefinition);

            if (null == values) values = new Dictionary<string, string>(new FilterNameComparer());
            else values = new Dictionary<string, string>(values, new FilterNameComparer());

            foreach (string parameterName in parameterNames)
            {
                if (context.ReportDefinition.TryFindFilterDefinition(parameterName, out FilterDefinition filterDefinition))
                {
                    if (!values.TryGetValue(parameterName, out string rawValue))
                    {
                        rawValue = null;
                    }

                    FilterValue filterValue = this.FilterValueBinder.GetFilterValue(filterDefinition, rawValue);
                }
            }

            return null;
        }
    }
}