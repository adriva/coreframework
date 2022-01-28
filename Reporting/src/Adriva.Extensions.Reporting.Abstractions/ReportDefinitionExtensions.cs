using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace Adriva.Extensions.Reporting.Abstractions
{
    public static class ReportDefinitionExtensions
    {
        public static IEnumerable<FilterDefinition> EnumerateFilterDefinitions(IDictionary<string, FilterDefinition> filterDefinitions)
        {
            if (null == filterDefinitions) yield break;

            foreach (var entry in filterDefinitions)
            {
                yield return entry.Value;

                foreach (var childDefinition in ReportDefinitionExtensions.EnumerateFilterDefinitions(entry.Value.Children))
                {
                    yield return childDefinition;
                }
            }
        }

        public static IEnumerable<FilterDefinition> EnumerateFilterDefinitions(this ReportDefinition reportDefinition)
        {
            return ReportDefinitionExtensions.EnumerateFilterDefinitions(reportDefinition?.Filters);
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

        // public static T Get<T>(this IDynamicDefinition dynamicDefinition) where T : class
        // {
        //     if (null == dynamicDefinition?.Options)
        //     {
        //         return null;
        //     }

        //     return dynamicDefinition.Options.Get<T>();
        // }
    }
}