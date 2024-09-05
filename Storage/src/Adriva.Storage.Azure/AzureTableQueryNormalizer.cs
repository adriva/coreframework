using System;
using System.Linq.Expressions;
using System.Reflection;
using Azure.Data.Tables;
using MsAzure = global::Azure;

namespace Adriva.Storage.Azure
{
    public class AzureTableQueryNormalizer : ExpressionVisitor
    {
        private readonly Type TypeOfETag = typeof(MsAzure.ETag);

        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.Member is PropertyInfo propertyInfo && propertyInfo.PropertyType.Equals(this.TypeOfETag))
            {
                throw new InvalidOperationException($"Querying with ETag type is not supported. {node}");
            }

            if (null != node.Expression && ExpressionType.Parameter == node.Expression.NodeType && node.Expression is ParameterExpression parameterExpression)
            {
                string propertyName = Helpers.GetPropertyName(node.Member, out bool isBaseTypeName);
                MemberExpression propertyExpression = null;

                if (isBaseTypeName)
                {
                    propertyExpression = Expression.PropertyOrField(Expression.Parameter(typeof(TableEntity), parameterExpression.Name), propertyName);
                }
                else
                {
                    propertyExpression = Expression.PropertyOrField(parameterExpression, propertyName);
                }

                return propertyExpression;
            }

            return base.VisitMember(node);
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            var leftExpression = this.Visit(node.Left);
            var rightExpression = this.Visit(node.Right);

            switch (node.NodeType)
            {
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                case ExpressionType.LessThan:
                case ExpressionType.GreaterThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.GreaterThanOrEqual:
                    if (leftExpression.Type != rightExpression.Type)
                    {
                        if (ExpressionType.MemberAccess == rightExpression.NodeType && rightExpression is MemberExpression memberExpression)
                        {
                            var mi = typeof(Convert).GetMethod(nameof(Convert.ChangeType), new[] { typeof(object), typeof(Type) });
                            var ee = Expression.Call(mi, Expression.Convert(memberExpression, typeof(object)), Expression.Constant(leftExpression.Type));
                            return Expression.MakeBinary(node.NodeType, leftExpression, ee);
                        }
                    }
                    break;
                default:
                    break;
            }

            return base.VisitBinary(node);
        }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            var baseExpression = base.Visit(node.Body);
            return Expression.Lambda(baseExpression, Expression.Parameter(typeof(TableEntity), node.Parameters[0].Name));
        }
    }
}
