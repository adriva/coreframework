using System.Collections;
using System.Collections.Generic;

namespace Adriva.Extensions.Reporting.Abstractions
{
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
}