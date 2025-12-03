using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Reflection;
using Newtonsoft.Json;

namespace Adriva.Common.Core.Serialization.Json
{
    public sealed class MappingBuilder<T> : IMappingBuilder
    {
        private readonly IDictionary<string, PropertyContract> PropertyContractMappings = new Dictionary<string, PropertyContract>(StringComparer.OrdinalIgnoreCase);

        public IReadOnlyDictionary<string, PropertyContract> PropertyContracts => new ReadOnlyDictionary<string, PropertyContract>(this.PropertyContractMappings);

        public MappingBuilder<T> MapProperty<U>(Expression<Func<T, U>> expression, string serializedName, bool ignoreDefaultValue = true, bool shouldNegate = false)
        {
            if (null == expression?.Body) throw new ArgumentNullException(nameof(expression));

            if (expression.Body is MemberExpression memberExpression && ExpressionType.MemberAccess == memberExpression.NodeType && MemberTypes.Property == memberExpression.Member.MemberType)
            {
                this.PropertyContractMappings.Add(memberExpression.Member.Name, new PropertyContract(serializedName)
                {
                    ShouldNegate = shouldNegate,
                    IgnoreDefaultValue = ignoreDefaultValue
                });
                return this;
            }

            throw new InvalidOperationException("Expression must be a valid property expression such as 'x => x.PropertyName'.");
        }

        public MappingBuilder<T> MapProperty<U>(Expression<Func<T, U>> expression, string serializedName, JsonConverter jsonConverter)
        {
            if (null == expression?.Body) throw new ArgumentNullException(nameof(expression));

            if (expression.Body is MemberExpression memberExpression && ExpressionType.MemberAccess == memberExpression.NodeType && MemberTypes.Property == memberExpression.Member.MemberType)
            {
                this.PropertyContractMappings.Add(memberExpression.Member.Name, new PropertyContract(serializedName)
                {
                    ShouldNegate = false,
                    IgnoreDefaultValue = false,
                    Converter = jsonConverter
                });
                return this;
            }

            throw new InvalidOperationException("Expression must be a valid property expression such as 'x => x.PropertyName'.");
        }
    }
}