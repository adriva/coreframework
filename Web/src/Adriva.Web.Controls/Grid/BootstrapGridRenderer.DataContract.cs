using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Adriva.Web.Controls.Abstractions;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Adriva.Common.Core.Serialization.Json;
using Newtonsoft.Json.Converters;

namespace Adriva.Web.Controls
{


    partial class BootstrapGridRenderer
    {

        private class GridContractResolver : DefaultContractResolver
        {
            private readonly IDictionary<Type, IMappingBuilder> MappingBuilders = new Dictionary<Type, IMappingBuilder>();

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
                            property.DefaultValueHandling = propertyContract.IgnoreDefaultValue ? DefaultValueHandling.Ignore : DefaultValueHandling.Include;

                            if (property.PropertyType.IsEnum)
                            {
                                property.Converter = new StringEnumConverter(new CamelCaseNamingStrategy(), true);
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

                return properties;
            }

            protected override JsonContract CreateContract(Type objectType)
            {
                return base.CreateContract(objectType);
            }

            protected override string ResolvePropertyName(string propertyName)
            {
                return propertyName;
            }
        }
    }
}