using System;
using System.Linq.Expressions;
using System.Reflection;
using Adriva.Storage.Abstractions;

namespace Adriva.Storage.Azure
{
    internal sealed class QueryExpressionVisitor<TItem> : DynamicExpressionVisitor
    {

        private readonly Type TypeOfTItem = typeof(TItem);

        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.Member.DeclaringType.Equals(this.TypeOfTItem))
            {
                if (null != node.Member.GetCustomAttribute<NotMappedAttribute>())
                {
                    throw new InvalidOperationException($"Property '{node.Member.Name}' declared in '{node.Member.DeclaringType.FullName}' has the NotMapped attribute and can not be used in table queries.");
                }
            }
            return node;
        }
    }
}
