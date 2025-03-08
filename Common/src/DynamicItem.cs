using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Adriva.Common.Core
{
    /// <summary>
    /// Represents a class that can have dynamic properties and be casted to dynamic type.
    /// </summary>
    [Serializable]
    [JsonArray]
    [JsonConverter(typeof(DynamicItem.DynamicItemConverter))]
    public class DynamicItem : DynamicObject, IEnumerable<KeyValuePair<string, object>>, ICloneable
    {

        internal sealed class DynamicItemConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType)
            {
                return typeof(DynamicItem).IsAssignableFrom(objectType);
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {

                if (JsonToken.Null == reader.TokenType) return new DynamicItem();

                var jo = JObject.Load(reader);

                DynamicItem dynamicItem = new DynamicItem();

                var enumerator = jo.Properties().GetEnumerator();

                while (enumerator.MoveNext())
                {
                    var current = enumerator.Current;
                    object value = null;

                    if (JTokenType.Integer == current.Value.Type)
                    {
                        value = current.Value.Value<int>();
                    }
                    else if (JTokenType.Float == current.Value.Type)
                    {
                        value = current.Value.Value<float>();
                    }
                    else if (JTokenType.Guid == current.Value.Type)
                    {
                        value = current.Value.Value<Guid>();
                    }
                    else if (JTokenType.String == current.Value.Type)
                    {
                        value = current.Value.Value<string>();
                    }
                    else if (JTokenType.Boolean == current.Value.Type)
                    {
                        value = current.Value.Value<bool>();
                    }
                    else
                    {
                        value = current.Value.Value<object>();
                    }

                    dynamicItem.Data.Add(DynamicItem.NormalizeKey(current.Name), value);
                }

                return dynamicItem;
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                DynamicItem dynamicItem = value as DynamicItem;

                if (null == dynamicItem) throw new NotSupportedException("Can only convert DynamicItem");

                writer.WriteStartObject();

                var enumerator = dynamicItem.Data.GetEnumerator();

                while (enumerator.MoveNext())
                {
                    var pair = enumerator.Current;
                    writer.WritePropertyName(pair.Key);
                    writer.WriteValue(pair.Value);
                }

                writer.WriteEndObject();
            }
        }

        private readonly Dictionary<string, object> Data = new Dictionary<string, object>();

        private static string NormalizeKey(string key)
        {
            if (null == key) return null;
            return key.ToUpperInvariant();
        }

        /// <summary>
        /// Gets the number of dynamic properties defined on this instance of the class.
        /// </summary>
        /// <value>The number of dynamic properties defined on this instance of the class.</value>
        public int Count
        {
            get { return this.Data.Count; }
        }

        /// <summary>
        /// Gets or sets the property values associated with the given dynamic property name.
        /// </summary>
        /// <value>The value of the dynamic property with the given name.</value>
        public object this[string key]
        {
            get { return this.Data[DynamicItem.NormalizeKey(key)]; }
            set { this.Data[DynamicItem.NormalizeKey(key)] = value; }
        }

        /// <summary>
        /// Initializes a new instance of DynamicItem class.
        /// </summary>
        [JsonConstructor]
        public DynamicItem() { }

        /// <summary>
        /// Initializes a new instance of DynamicItem class.
        /// </summary>
        /// <param name="items">A key/value collection representing the dynamic property names and their values, to create the class dynamic properties with.</param>
        public DynamicItem(IDictionary<string, object> items)
        {
            if (null == items) return;

            foreach (var item in items)
            {
                string normalizedKey = DynamicItem.NormalizeKey(item.Key);
                this.Data.Add(normalizedKey, item.Value);
            }
        }

        /// <summary>
        /// Checks if the given dynamic property exists on this instance of the class.
        /// </summary>
        /// <param name="key">The name of the property to be sought.</param>
        /// <returns>True if the property name exists, otherwise False.</returns>
        public bool ContainsKey(string key)
        {
            if (null == key) return false;
            key = DynamicItem.NormalizeKey(key);
            return this.Data.ContainsKey(key);
        }

        /// <summary>
        /// Gets the value of a dynamic property with the given name if found, otherwise returns the default value given.
        /// </summary>
        /// <param name="key">The name of the dynamic property to be retrieved.</param>
        /// <param name="defaultValue">The default value that will be returned if the dynamic property does not exist.</param>
        /// <typeparam name="T">The type of the property value.</typeparam>
        /// <returns>The value of the dynamic property.</returns>
        public T GetValue<T>(string key, T defaultValue)
        {
            if (!this.ContainsKey(key))
            {
                return defaultValue;
            }

            return (T)this[key];
        }

        /// <summary>
        /// Removes all dynamic properties from the dynamic object.
        /// </summary>
        public void Clear()
        {
            this.Data.Clear();
        }

        #region IEnumerable Implementation

        /// <summary>
        /// Returns an enumerator that iterates through the dynamic properties.
        /// </summary>
        /// <returns>An enumerator structure for the dynamic properties.</returns>
        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return this.Data.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the dynamic properties.
        /// </summary>
        /// <returns>An enumerator structure for the dynamic properties.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.Data.GetEnumerator();
        }

        #endregion

        #region Dynamic Object Implementation

        /// <summary>
        /// Casts the current DynamicItem to a dynamic object to support accessing the dynamic properties using the language semantics.
        /// </summary>
        /// <value>A dynamic type that has all the properties defined on the DynamicItem class.</value>
        public dynamic Dynamic
        {
            get
            {
                return (dynamic)this;
            }
        }

        /// <summary>
        /// Gets a collection containing the dynamic property names defined.
        /// </summary>
        /// <returns>An enumerable list of strings representing the dynamic property names.</returns>
        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return this.Data.Keys;
        }

        /// <summary>
        /// Provides the implementation for operations that get member values. Classes derived from the System.Dynamic.DynamicObject class can override this method to specify dynamic behavior for operations such as getting a value for a property.
        /// </summary>
        /// <param name="binder">Provides information about the object that called the dynamic operation. The binder.Name property provides the name of the member on which the dynamic operation is performed. For example, for the Console.WriteLine(sampleObject.SampleProperty) statement, where sampleObject is an instance of the class derived from the System.Dynamic.DynamicObject class, binder.Name returns "SampleProperty". The binder.IgnoreCase property specifies whether the member name is case-sensitive.</param>
        /// <param name="result">The result of the get operation.</param>
        /// <returns>True if the operation is successful; otherwise, False. If this method returns False, the run-time binder of the language determines the behavior. (In most cases, a run-time exception is thrown.)</returns>
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            var normalizedKey = DynamicItem.NormalizeKey(binder.Name);
            result = null;

            if (this.Data.ContainsKey(normalizedKey))
            {
                result = this.Data[normalizedKey];
                return true;
            }
            return false;
        }

        /// <summary>
        /// Provides the implementation for operations that set member values.
        /// </summary>
        /// <param name="binder">Provides information about the object that called the dynamic operation. The binder.Name property provides the name of the member to which the value is being assigned.</param>
        /// <param name="value">The value to set to the member.</param>
        /// <returns>True if the operation is successful; otherwise, False. If this method returns False, the run-time binder of the language determines the behavior. (In most cases, a language-specific run-time exception is thrown.)</returns>
        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            var normalizedKey = DynamicItem.NormalizeKey(binder.Name);

            this.Data[normalizedKey] = value;

            return true;
        }

        #endregion

        #region ICloneable Implementation

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>A new object that is a copy of this instance.</returns>
        public object Clone()
        {
            DynamicItem clone = new DynamicItem();

            foreach (var dataItem in this.Data)
            {
                if (dataItem.Value is ICloneable cloneableValue)
                {
                    clone.Data.Add(dataItem.Key, cloneableValue.Clone());
                }
                else clone.Data.Add(dataItem.Key, dataItem.Value);
            }

            return clone;
        }

        #endregion

    }
}
