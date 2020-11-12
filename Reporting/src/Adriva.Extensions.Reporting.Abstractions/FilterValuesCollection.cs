using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace Adriva.Extensions.Reporting.Abstractions
{
    public class FilterValuesCollection : IDictionary<string, string>
    {
        private readonly IDictionary<string, string> Values;

        public ICollection<string> Keys => this.Values.Keys;

        ICollection<string> IDictionary<string, string>.Values => this.Values.Values;

        public int Count => this.Values.Count;

        public bool IsReadOnly => this.Values.IsReadOnly;

        public string this[string key]
        {
            get => this.Values.TryGetValue(key, out string value) ? value : null;
            set => this.Values[key] = value;
        }

        public FilterValuesCollection()
        {
            this.Values = new Dictionary<string, string>(new FilterNameComparer());
        }

        public void Add(string key, string value)
        {
            this.Values.Add(key, value);
        }

        public bool ContainsKey(string key)
        {
            return this.Values.ContainsKey(key);
        }

        public bool Remove(string key)
        {
            return this.Values.Remove(key);
        }

        public bool TryGetValue(string key, out string value)
        {
            return this.Values.TryGetValue(key, out value);
        }

        public void Add(KeyValuePair<string, string> item)
        {
            this.Values.Add(item);
        }

        public void Clear()
        {
            this.Values.Clear();
        }

        public bool Contains(KeyValuePair<string, string> item)
        {
            return this.Values.Contains(item);
        }

        public void CopyTo(KeyValuePair<string, string>[] array, int arrayIndex)
        {
            this.Values.CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<string, string> item)
        {
            return this.Values.Remove(item);
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return this.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)Values).GetEnumerator();
        }

        public bool TryGetValue(FilterDefinition filterDefinition, out object value)
        {
            if (null == filterDefinition) throw new ArgumentNullException(nameof(filterDefinition), $"FilterDefinition is not set to an instance of and object.");
            if (string.IsNullOrWhiteSpace(filterDefinition.Name)) throw new ArgumentException($"FilterDefinition doesn't have a name.");

            string filterValue = this[filterDefinition.Name];

            if (null == filterValue)
            {
                value = null;
            }

            value = Convert.ChangeType(filterValue, filterDefinition.DataType, CultureInfo.CurrentCulture);

            return true;
        }
    }
}