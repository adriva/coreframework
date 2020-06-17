using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Adriva.Storage.Abstractions;
using Microsoft.Azure.Cosmos.Table;

namespace Adriva.Storage.Azure
{
    internal sealed class TableItemAssembler : ITableItemAssembler
    {
        private readonly static MethodInfo CastMethod;
        private readonly static MethodInfo CreateEntityPropertyMethod;
        private readonly IDictionary<Type, Action<object, DynamicTableEntity>> AssemblerCache = new Dictionary<Type, Action<object, DynamicTableEntity>>();
        private readonly IDictionary<Type, Action<object, DynamicTableEntity>> DisassemblerCache = new Dictionary<Type, Action<object, DynamicTableEntity>>();

        static TableItemAssembler()
        {
            TableItemAssembler.CastMethod = typeof(TableItemAssembler).GetMethod("CastEntityProperty", BindingFlags.Static | BindingFlags.NonPublic);
            TableItemAssembler.CreateEntityPropertyMethod = typeof(EntityProperty).GetMethod("CreateEntityPropertyFromObject", BindingFlags.Static | BindingFlags.Public);
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

        public TItem Assemble<TItem>(DynamicTableEntity tableEntity) where TItem : class, ITableItem, new()
        {
            if (null == tableEntity) return default;

            Type typeOfT = typeof(TItem);

            // tableEntity.Properties.TryAdd("PartitionKey", EntityProperty.GeneratePropertyForString(tableEntity.PartitionKey));
            // tableEntity.Properties.TryAdd("RowKey", EntityProperty.GeneratePropertyForString(tableEntity.RowKey));
            tableEntity.Properties.TryAdd("Timestamp", EntityProperty.GeneratePropertyForDateTimeOffset(tableEntity.Timestamp));
            tableEntity.Properties.TryAdd("Etag", EntityProperty.GeneratePropertyForString(tableEntity.ETag));

            Action<object, DynamicTableEntity> populateAction = null;

            if (!this.AssemblerCache.TryGetValue(typeOfT, out populateAction))
            {
                var properties = typeOfT.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                var mappings = TableItemAssembler.GetPropertyMappings(properties, tableEntity.Properties.Keys);

                foreach (var mapping in mappings)
                {
                    foreach (var property in mapping.Value)
                    {
                        var castMethod = TableItemAssembler.CastMethod.MakeGenericMethod(property.PropertyType);
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
                this.AssemblerCache.Add(typeOfT, populateAction);
            }

            TItem item = new TItem();
            item.PartitionKey = tableEntity.PartitionKey;
            item.RowKey = tableEntity.RowKey;
            populateAction.Invoke(item, tableEntity);

            if (item is ITableEntity azureTableEntity)
            {
                azureTableEntity.ReadEntity(tableEntity.Properties, null);
            }

            return item;
        }

        public ITableEntity Disassemble<TItem>(TItem item) where TItem : class, ITableItem
        {
            if (null == item) throw new ArgumentNullException(nameof(item));

            Type typeOfT = typeof(TItem);
            Action<object, DynamicTableEntity> wrapperAction = null;

            DynamicTableEntity tableEntity = new DynamicTableEntity();

            if (!this.DisassemblerCache.TryGetValue(typeOfT, out wrapperAction))
            {

                Action<TItem, DynamicTableEntity> propertyPopulator = null;
                var properties = typeOfT.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                foreach (var property in properties)
                {
                    if (null != property.GetCustomAttribute<NotMappedAttribute>()) continue;
                    if (!property.CanRead || !property.CanWrite) continue;


                    var itemParameter = Expression.Parameter(typeOfT, "x");
                    var propertyAccessor = Expression.Property(itemParameter, property);
                    var dynamicEntityParameter = Expression.Parameter(typeof(DynamicTableEntity), "d");

                    if (0 != string.Compare("PartitionKey", property.Name, StringComparison.OrdinalIgnoreCase)
                        && 0 != string.Compare("RowKey", property.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        var dynamicEntityProperties = Expression.Property(dynamicEntityParameter, "Properties");

                        var objectPropertyAccessor = Expression.Convert(propertyAccessor, typeof(object));
                        var createEntityPropertyExpression = Expression.Call(null, TableItemAssembler.CreateEntityPropertyMethod, objectPropertyAccessor);

                        var addPropertyExpression = Expression.Call(dynamicEntityProperties, "Add", null, Expression.Constant(property.Name), createEntityPropertyExpression);

                        var lambda = Expression.Lambda<Action<TItem, DynamicTableEntity>>(addPropertyExpression, itemParameter, dynamicEntityParameter);

                        if (null == propertyPopulator) propertyPopulator = lambda.Compile();
                        else propertyPopulator += lambda.Compile();
                    }
                }

                Action<TItem, DynamicTableEntity> basePropertyPopulator = (x, d) =>
                {
                    d.PartitionKey = x.PartitionKey;
                    d.RowKey = x.RowKey;
                };

                if (null == propertyPopulator) propertyPopulator = basePropertyPopulator;
                else propertyPopulator += basePropertyPopulator;

                wrapperAction = (objectItem, dynamicExpressionVisitor) =>
                {
                    TItem inputItem = (TItem)objectItem;
                    propertyPopulator.Invoke(inputItem, dynamicExpressionVisitor);
                };

                this.DisassemblerCache.Add(typeOfT, wrapperAction);
            }

            wrapperAction.Invoke(item, tableEntity);
            return tableEntity;
        }

    }
}
