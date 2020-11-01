using System;
using System.Collections.Generic;
using Adriva.Common.Core.Serialization.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Adriva.Web.Controls.Abstractions
{
    public class ControlContractResolver : DefaultContractResolver
    {
        private readonly IDictionary<Type, IMappingBuilder> MappingBuilders = new Dictionary<Type, IMappingBuilder>();

        protected virtual IList<JsonProperty> GetDynamicProperties(Type ownerType)
        {
            return null;
        }

        public MappingBuilder<T> AddTypeMapping<T>() where T : ControlTagHelper
        {
            Type typeOfT = typeof(T);
            if (this.MappingBuilders.ContainsKey(typeOfT)) return (MappingBuilder<T>)this.MappingBuilders[typeOfT];
            var mappingBuilder = new MappingBuilder<T>();
            this.MappingBuilders.Add(typeOfT, mappingBuilder);
            return mappingBuilder;
        }

        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            var properties = base.CreateProperties(type, memberSerialization);
            Queue<JsonProperty> deleteQueue = new Queue<JsonProperty>();

            foreach (var property in properties)
            {
                if (this.MappingBuilders.TryGetValue(property.DeclaringType, out IMappingBuilder mappingBuilder))
                {
                    if (mappingBuilder.PropertyContracts.TryGetValue(property.UnderlyingName, out PropertyContract propertyContract))
                    {
                        property.Ignored = false;
                        property.PropertyName = propertyContract.OverridenName;
                        if (!property.DefaultValueHandling.HasValue)
                        {
                            property.DefaultValueHandling = propertyContract.IgnoreDefaultValue ? DefaultValueHandling.Ignore : DefaultValueHandling.Include;
                        }

                        if (property.PropertyType.IsEnum)
                        {
                            property.Converter = new StringEnumConverter(new CamelCaseNamingStrategy(), true);
                        }

                        if (null != propertyContract.Converter)
                        {
                            property.Converter = propertyContract.Converter;
                        }

                        if (propertyContract.ShouldNegate && property.PropertyType.Equals(typeof(bool)))
                        {
                            property.ValueProvider = new NegatingValueProvider(property.ValueProvider);
                        }
                    }
                    else
                    {
                        deleteQueue.Enqueue(property);
                    }
                }
                else
                {
                    deleteQueue.Enqueue(property);
                }


            }

            while (0 < deleteQueue.Count)
            {
                properties.Remove(deleteQueue.Dequeue());
            }

            var dynamicProperties = this.GetDynamicProperties(type);

            if (null != dynamicProperties)
            {
                foreach (var dynamicProperty in dynamicProperties)
                {
                    properties.Add(dynamicProperty);
                }
            }

            return properties;
        }
    }
}