using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using Adriva.Common.Core;

namespace Adriva.Extensions.Worker.Durable;

public sealed class EvaluatingExpressionVisitor(Expression? container) : ExpressionVisitor
{
    private readonly Expression? Container = container;

    [return: NotNullIfNotNull("node")]
    public override Expression? Visit(Expression? node)
    {
        node = base.Visit(node);

        if (node is null)
        {
            return Expression.Constant(null);
        }
        else if (node is ConstantExpression constant)
        {
            return constant;
        }

        var g = Expression.Lambda(node);
        var dele = g.Compile();
        var hh = dele.DynamicInvoke();

        return Expression.Constant(hh);
    }
}

public sealed class DurableExpressionVisitor<TContainer> : ExpressionVisitor
{
    public static WorkerTaskOrchestration GetOrchestration(Expression<Func<TContainer, Task>> expression)
    {
        if (1 != expression.Parameters.Count)
        {
            throw new InvalidExpressionException("params = 1");
        }

        if (expression.Body is not MethodCallExpression)
        {
            throw new InvalidExpressionException("not method call");
        }

        DurableExpressionVisitor<TContainer> durableExpressionVisitor = new();

        var normalizedExpression = durableExpressionVisitor.Visit(expression);

        if (normalizedExpression is LambdaExpression lambdaExpression)
        {
            if (lambdaExpression.Body is MethodCallExpression methodCallExpression)
            {
                return new(methodCallExpression.Method.GetNormalizedName(), methodCallExpression.Method);
            }
        }

        return null;
    }

    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
        if (node.CanReduce)
        {
            return node.Reduce();
        }

        List<Expression> evaluatedArguments = [];

        foreach (Expression argument in node.Arguments)
        {
            EvaluatingExpressionVisitor evaluatingExpressionVisitor = new(node.Object);
            evaluatedArguments.Add(evaluatingExpressionVisitor.Visit(argument));
        }

        return Expression.Call(node.Object, node.Method, evaluatedArguments);
    }
}
