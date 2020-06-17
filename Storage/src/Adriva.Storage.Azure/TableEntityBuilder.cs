using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using Adriva.Common.Core;
using Adriva.Storage.Abstractions;
using Microsoft.Azure.Cosmos.Table;

namespace Adriva.Storage.Azure
{
    internal class TableEntityBuilder
    {
        private readonly static MethodInfo CastMethod;
        private readonly IDictionary<Type, Action<object, DynamicTableEntity>> MapActionsCache = new Dictionary<Type, Action<object, DynamicTableEntity>>();

        static TableEntityBuilder()
        {
            TableEntityBuilder.CastMethod = typeof(TableEntityBuilder).GetMethod("CastEntityProperty", BindingFlags.Static | BindingFlags.NonPublic);
        }

        private static TType CastEntityProperty<TType>(EntityProperty entityProperty)
        {
            if (typeof(TType) == typeof(object)) return (TType)entityProperty.PropertyAsObject;

            switch (entityProperty.PropertyType)
            {
                case EdmType.Binary:
                    return (TType)Convert.ChangeType(entityProperty.BinaryValue, typeof(TType));
                case EdmType.Boolean:
                    return (TType)Convert.ChangeType(entityProperty.BooleanValue, typeof(TType));
                case EdmType.DateTime:
                    if (typeof(TType) == typeof(DateTime))
                    {
                        return (TType)Convert.ChangeType(entityProperty.DateTime, typeof(TType));
                    }
                    else if (typeof(TType) == typeof(DateTimeOffset))
                    {
                        return (TType)Convert.ChangeType(entityProperty.DateTimeOffsetValue, typeof(TType));
                    }
                    break;
                case EdmType.Double:
                    return (TType)Convert.ChangeType(entityProperty.DoubleValue, typeof(TType));
                case EdmType.Guid:
                    return (TType)Convert.ChangeType(entityProperty.GuidValue, typeof(TType));
                case EdmType.Int32:
                    return (TType)Convert.ChangeType(entityProperty.Int32Value, typeof(TType));
                case EdmType.Int64:
                    return (TType)Convert.ChangeType(entityProperty.Int64Value, typeof(TType));
                case EdmType.String:
                    return (TType)Convert.ChangeType(entityProperty.StringValue, typeof(TType));
            }

            throw new InvalidCastException($"Table field of type '{entityProperty.PropertyType}' could not be mapped to property type '{typeof(TType).FullName}'.");
        }

        private static IDictionary<string, List<PropertyInfo>> GetPropertyMappings(IEnumerable<PropertyInfo> properties, IEnumerable<string> propertyNames)
        {
            Dictionary<string, List<PropertyInfo>> mappings = new Dictionary<string, List<PropertyInfo>>();

            if (null == properties || null == propertyNames || !properties.Any()) return mappings;

            foreach (var property in properties)
            {
                if (property.IsSpecialName) continue;

                if (null != property.GetCustomAttribute<IgnorePropertyAttribute>(true)) continue; //ignored (not mapped)

                NotMappedAttribute notMappedAttribute = property.GetCustomAttribute<NotMappedAttribute>();

                if (null != notMappedAttribute) continue; //if not mapped then ignore

                string propertyName = propertyNames.FirstOrDefault(pn => 0 == string.Compare(pn, property.Name, StringComparison.OrdinalIgnoreCase));
                if (null != propertyName)
                {
                    if (!mappings.ContainsKey(propertyName)) mappings[propertyName] = new List<PropertyInfo>();
                    mappings[propertyName].Add(property);
                }
            }

            return mappings;
        }

        public TItem Build<TItem>(DynamicTableEntity tableEntity) where TItem : class, new()
        {
            if (null == tableEntity) return default;

            Type typeOfT = typeof(TItem);

            tableEntity.Properties.TryAdd("PartitionKey", EntityProperty.GeneratePropertyForString(tableEntity.PartitionKey));
            tableEntity.Properties.TryAdd("RowKey", EntityProperty.GeneratePropertyForString(tableEntity.RowKey));
            tableEntity.Properties.TryAdd("Timestamp", EntityProperty.GeneratePropertyForDateTimeOffset(tableEntity.Timestamp));
            tableEntity.Properties.TryAdd("Etag", EntityProperty.GeneratePropertyForString(tableEntity.ETag));

            Action<object, DynamicTableEntity> populateAction = null;

            if (!this.MapActionsCache.TryGetValue(typeOfT, out populateAction))
            {
                var properties = typeOfT.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                var mappings = TableEntityBuilder.GetPropertyMappings(properties, tableEntity.Properties.Keys);

                foreach (var mapping in mappings)
                {
                    foreach (var property in mapping.Value)
                    {
                        var castMethod = TableEntityBuilder.CastMethod.MakeGenericMethod(property.PropertyType);
                        var itemParameter = Expression.Parameter(typeOfT, "x");
                        var itemProperty = Expression.Property(itemParameter, property.Name);
                        var propertyValue = Expression.Parameter(typeof(EntityProperty), "value");
                        var castEntityProperty = Expression.Call(null, castMethod, propertyValue);
                        var body = Expression.Assign(itemProperty, castEntityProperty);
                        var exp = Expression.Lambda<Action<TItem, EntityProperty>>(body, itemParameter, propertyValue);

                        Action<TItem, EntityProperty> mapAction = exp.Compile();

                        Action<object, DynamicTableEntity> wrapperAction = (objectItem, dynamicTableEntity) =>
                        {
                            TItem item = (TItem)objectItem;
                            mapAction.Invoke(item, dynamicTableEntity.Properties[mapping.Key]);
                        };

                        if (null == populateAction) populateAction = wrapperAction;
                        else populateAction += wrapperAction;
                    }
                }
                this.MapActionsCache.Add(typeOfT, populateAction);
            }

            TItem item = new TItem();
            populateAction.Invoke(item, tableEntity);
            return item;
        }
    }
}
