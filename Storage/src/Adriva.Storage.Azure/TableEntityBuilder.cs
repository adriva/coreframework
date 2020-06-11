using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Azure.Cosmos.Table;

namespace Adriva.Storage.Azure
{
    internal class TableEntityBuilder
    {
        private static TType CastEntityProperty<TType>(EntityProperty entityProperty)
        {
            return default;
        }

        public TItem Build<TItem>(DynamicTableEntity tableEntity) where TItem : class
        {
            if (null == tableEntity) return default;

            Type typeOfT = typeof(TItem);

            var properties = typeOfT.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            tableEntity.Properties.Add("PartitionKey", EntityProperty.GeneratePropertyForString(tableEntity.PartitionKey));
            tableEntity.Properties.Add("RowKey", EntityProperty.GeneratePropertyForString(tableEntity.RowKey));

            foreach (var tableProperty in tableEntity.Properties)
            {
                var objectProperty = properties.FirstOrDefault(p => 0 == string.Compare(p.Name, tableProperty.Key, StringComparison.OrdinalIgnoreCase));

                if (null != objectProperty)
                {
                    var castMethod = typeof(TableEntityBuilder).GetMethod("CastEntityProperty", BindingFlags.Static | BindingFlags.NonPublic);
                    castMethod = castMethod.MakeGenericMethod(objectProperty.PropertyType);
                    var itemParameter = Expression.Parameter(typeOfT, "x");
                    var itemProperty = Expression.Property(itemParameter, "PartitionKey");
                    var propertyValue = Expression.Parameter(typeof(EntityProperty), "value");
                    var castEntityProperty = Expression.Call(null, castMethod, propertyValue);
                    var body = Expression.Assign(itemProperty, castEntityProperty);
                    var exp = Expression.Lambda<Action<TItem, EntityProperty>>(body, itemParameter, propertyValue);
                    System.Console.WriteLine($"{objectProperty.Name}  : " + exp.ToString());
                }
            }


            return default;
        }
    }
}
