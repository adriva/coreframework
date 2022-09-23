using System;
using System.Collections.Generic;
using System.Linq;

namespace Adriva.Extensions.Reporting.Abstractions
{
    public static class ReportingExtensions
    {
        public static FilterDefinition FindFilter(this ReportDefinition reportDefinition, string filterName)
        {
            return reportDefinition
                        .EnumerateFilterDefinitions()
                        .FirstOrDefault(x => 0 == string.Compare(filterName, x.Name, StringComparison.OrdinalIgnoreCase));
        }

        public static ReportCommandParameter FindParameter(this ReportCommand reportCommand, string filterName)
        {
            if (null == reportCommand?.Parameters)
            {
                return null;
            }

            return reportCommand.Parameters.FirstOrDefault(x => 0 == string.Compare(x.Name, filterName, StringComparison.OrdinalIgnoreCase));
        }

        public static IEnumerable<FilterDefinition> EnumerateFilterDefinitions(IDictionary<string, FilterDefinition> filterDefinitions)
        {
            if (null == filterDefinitions) yield break;

            foreach (var entry in filterDefinitions)
            {
                yield return entry.Value;

                foreach (var childDefinition in ReportingExtensions.EnumerateFilterDefinitions(entry.Value.Children))
                {
                    yield return childDefinition;
                }
            }
        }

        public static IEnumerable<FilterDefinition> EnumerateFilterDefinitions(this ReportDefinition reportDefinition)
        {
            return ReportingExtensions.EnumerateFilterDefinitions(reportDefinition?.Filters);
        }

        public static bool TryFindFilterDefinition(this ReportDefinition reportDefinition, string filterName, out FilterDefinition filterDefinition)
        {
            filterDefinition = null;
            if (null == reportDefinition) return false;

            Queue<IDictionary<string, FilterDefinition>> queue = new Queue<IDictionary<string, FilterDefinition>>();
            queue.Enqueue(reportDefinition.Filters);

            while (0 < queue.Count)
            {
                var entries = queue.Dequeue();
                if (entries.TryGetValue(filterName, out filterDefinition)) return true;

                foreach (var entry in entries)
                {
                    if (null != entry.Value.Children)
                    {
                        queue.Enqueue(entry.Value.Children);
                    }
                }
            }

            return false;
        }

        public static bool TryFindDataSourceDefinition(this ReportDefinition reportDefinition, IDataDrivenObject dataDrivenObject, out DataSourceDefinition dataSourceDefinition)
        {
            dataSourceDefinition = null;
            if (null == reportDefinition) return false;
            if (string.IsNullOrWhiteSpace(dataDrivenObject?.DataSource)) return false;
            return reportDefinition.DataSources.TryGetValue(dataDrivenObject.DataSource, out dataSourceDefinition);
        }

        public static IEnumerable<FieldDefinition> EnumerateFieldDefinitions(this ReportDefinition reportDefinition)
        {
            if (null == reportDefinition?.Output?.Fields) yield break;

            foreach (var fieldDefinitionEntry in reportDefinition.Output.Fields)
            {
                yield return fieldDefinitionEntry.Value;
            }
        }

        public static IEnumerable<FieldDefinition> EnumerateFieldDefinitions(this FilterDefinition filterDefinition)
        {
            if (null == filterDefinition?.Fields) yield break;

            foreach (var fieldDefinitionEntry in filterDefinition.Fields)
            {
                yield return fieldDefinitionEntry.Value;
            }
        }

        public static string GetNameWithoutParameters(this ReportCommand command)
        {
            if (null == command?.CommandDefinition?.CommandText)
            {
                return null;
            }

            return new string(command.CommandDefinition.CommandText.TakeWhile(x => '(' != x && ' ' != x && '@' != x).ToArray());
        }

        public static T Get<T>(this IDynamicDefinition dynamicDefinition) where T : class
        {
            if (null == dynamicDefinition?.Options)
            {
                return null;
            }

            return dynamicDefinition.Options.ToObject<T>();
        }
    }
}