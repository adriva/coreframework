using System.Collections.Generic;

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
    }
}