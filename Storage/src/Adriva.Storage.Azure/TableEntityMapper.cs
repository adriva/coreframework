using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Azure.Data.Tables;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MsAzure = global::Azure;

namespace Adriva.Storage.Azure
{
    public class TableEntityMapper<T> : ITableEntityMapper<T> where T : class
    {
        private static readonly Expression<Func<object, DateTime?>> NullableDateTimeConverter = x => (
                                                                                                    x is DateTimeOffset? ?
                                                                                                        (((DateTimeOffset?)x).HasValue ?
                                                                                                            DateTime.SpecifyKind(((DateTimeOffset?)x).Value.DateTime, DateTimeKind.Utc) :
                                                                                                                (DateTime?)null)
                                                                                                        :
                                                                                                        (
                                                                                                            x is DateTimeOffset ?
                                                                                                                DateTime.SpecifyKind(((DateTimeOffset)x).DateTime, DateTimeKind.Utc) :
                                                                                                                    (DateTime?)null
                                                                                                        )
                                                                                                );

        private static readonly Expression<Func<object, DateTime>> DateTimeConverter = x => (
                                                                                                x is DateTimeOffset? ?
                                                                                                    (((DateTimeOffset?)x).HasValue ?
                                                                                                        DateTime.SpecifyKind(((DateTimeOffset?)x).Value.DateTime, DateTimeKind.Utc) :
                                                                                                            DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc))
                                                                                                    :
                                                                                                    (
                                                                                                        x is DateTimeOffset ?
                                                                                                            DateTime.SpecifyKind(((DateTimeOffset)x).DateTime, DateTimeKind.Utc) :
                                                                                                                DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc)
                                                                                                    )
                                                                                            );

        private readonly ILogger Logger;
        protected IServiceProvider ServiceProvider { get; private set; }
        private Action<TableEntity, T> IngressMapperDelegate;
        private Action<T, TableEntity> EgressMapperDelegate;

        public TableEntityMapper(IServiceProvider serviceProvider)
        {
            this.ServiceProvider = serviceProvider;
            this.Logger = this.ServiceProvider.GetRequiredService<ILogger<TableEntityMapper<T>>>();

            if (!typeof(IAzureTableMapper).IsAssignableFrom(typeof(T)))
            {
                this.ResolveIngressPropertyMappings();
                this.ResolveEgressPropertyMappings();
            }
        }

        protected virtual string ResolveEntityKey(PropertyInfo propertyInfo)
        {
            string key = Helpers.GetPropertyName(propertyInfo, out _);
            if (0 == string.Compare(nameof(ITableEntity.ETag), key, StringComparison.OrdinalIgnoreCase))
            {
                return "odata.etag";
            }

            return key;
        }

        private Expression ConvertOffsetToDateTime(ParameterExpression parameterExpression, bool isPropertyNullable)
        {
            if (isPropertyNullable)
            {
                return Expression.Invoke(TableEntityMapper<T>.NullableDateTimeConverter, parameterExpression);
            }
            else
            {
                return Expression.Invoke(TableEntityMapper<T>.DateTimeConverter, parameterExpression);
            }
        }

        protected virtual IEnumerable<PropertyInfo> GetValidProperties()
        {
            return typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(p => !p.IsSpecialName);
        }

        protected virtual void ResolveIngressPropertyMappings()
        {
            this.Logger.LogDebug($"Resolving property mappings for type '{typeof(T).FullName}'.");

            var properties = this.GetValidProperties();

            var tableEntityParameter = Expression.Parameter(typeof(TableEntity), "tableEntity");
            var dataItemParameter = Expression.Parameter(typeof(T), "dataItem");

            foreach (var property in properties)
            {
                if (!property.CanWrite)
                {
                    continue;
                }

                var propertyValue = Expression.Variable(typeof(object), "value");
                var dataItemProperty = Expression.Property(dataItemParameter, property.Name);
                var invokeTryGetValue = Expression.Call(tableEntityParameter, nameof(TableEntity.TryGetValue), null, Expression.Constant(this.ResolveEntityKey(property)), propertyValue);
                Expression finalValue = propertyValue;

                if (property.PropertyType == typeof(DateTime?) || property.PropertyType == typeof(DateTime))
                {
                    finalValue = this.ConvertOffsetToDateTime(propertyValue, property.PropertyType == typeof(DateTime?));
                }
                else if (property.PropertyType == typeof(MsAzure.ETag))
                {
                    var etagCtor = typeof(MsAzure.ETag).GetConstructor(new[] { typeof(string) });
                    finalValue = Expression.New(etagCtor, Expression.Convert(propertyValue, typeof(string)));
                }

                Expression getKeyAndAssignValue = Expression.IfThen(invokeTryGetValue, Expression.Assign(dataItemProperty, Expression.Convert(finalValue, dataItemProperty.Type)));
                var propertyBlock = Expression.Block(typeof(void), new[] { propertyValue }, getKeyAndAssignValue);
                Expression<Action<TableEntity, T>> propertyLambda = (Expression<Action<TableEntity, T>>)Expression.Lambda(propertyBlock, new[] { tableEntityParameter, dataItemParameter });

                this.Logger.LogDebug($"Mapping action for property '{property.Name}' is: {getKeyAndAssignValue}");

                if (null == this.IngressMapperDelegate)
                {
                    this.IngressMapperDelegate = propertyLambda.Compile();
                }
                else
                {
                    this.IngressMapperDelegate += propertyLambda.Compile();
                }
            }
        }

        protected virtual void ResolveEgressPropertyMappings()
        {
            var properties = this.GetValidProperties();

            var tableEntityParameter = Expression.Parameter(typeof(TableEntity), "tableEntity");
            var dataItemParameter = Expression.Parameter(typeof(T), "dataItem");

            foreach (var property in properties)
            {
                if (!property.CanRead)
                {
                    continue;
                }

                string propertyName = Helpers.GetPropertyName(property, out bool isBaseTypeName);
                BinaryExpression propertyAssignment = null;

                if (isBaseTypeName)
                {
                    propertyAssignment = Expression.Assign(Expression.Property(tableEntityParameter, propertyName), Expression.Property(dataItemParameter, property.Name));
                }
                else
                {
                    propertyAssignment = Expression.Assign(Expression.Property(tableEntityParameter, "Item", Expression.Constant(property.Name)), Expression.Convert(Expression.Property(dataItemParameter, property.Name), typeof(object)));
                }

                Expression<Action<T, TableEntity>> propertyLambda = Expression.Lambda<Action<T, TableEntity>>(propertyAssignment, new[] { dataItemParameter, tableEntityParameter });

                if (null == this.EgressMapperDelegate)
                {
                    this.EgressMapperDelegate = propertyLambda.Compile();
                }
                else
                {
                    this.EgressMapperDelegate += propertyLambda.Compile();
                }
            }
        }

        protected virtual T CreateInstance()
        {
            return ActivatorUtilities.CreateInstance<T>(this.ServiceProvider);
        }

        public virtual T Build(TableEntity tableEntity)
        {
            try
            {
                T instance = this.CreateInstance();

                if (instance is IAzureTableMapper azureTableMapper)
                {
                    azureTableMapper.Read(tableEntity);
                }
                else
                {
                    this.IngressMapperDelegate(tableEntity, instance);
                }

                return instance;
            }
            catch (Exception fatalError)
            {
                this.Logger.LogError(fatalError, $"Failed to build output object of type '{typeof(T).FullName}'.");
                throw;
            }
        }

        public virtual TableEntity Build(T item)
        {
            TableEntity tableEntity = new TableEntity();

            if (item is IAzureTableMapper azureTableMapper)
            {
                azureTableMapper.Write(tableEntity);
            }
            else
            {
                this.EgressMapperDelegate(item, tableEntity);
            }

            return tableEntity;
        }
    }
}
