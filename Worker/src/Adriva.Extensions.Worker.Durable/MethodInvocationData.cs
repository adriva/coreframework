#if MID
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq.Expressions;
using Adriva.Common.Core;

namespace Adriva.Extensions.Worker.Durable;

[DebuggerDisplay("{ShortName}")]
public readonly struct MethodInvocationData<TContainer>
{
    private readonly List<object?> ParameterList = [];

    public string Name { get; }

    public string ShortName { get; }

    public Func<TContainer, Task>? CompiledMethod { get; }

    public ImmutableList<object?> Parameters => [.. this.ParameterList];

    public MethodInvocationData(Expression<Func<TContainer, Task>> lambdaExpression)
    {
        MethodCallExpression methodCallExpression = (MethodCallExpression)lambdaExpression.Body;
        this.Name = $"{ReflectionHelpers.GetNormalizedName(methodCallExpression.Method.DeclaringType)}::{ReflectionHelpers.GetNormalizedName(methodCallExpression.Method)}";
        this.ShortName = $"{ReflectionHelpers.GetNormalizedName(methodCallExpression.Method.DeclaringType, true)}::{ReflectionHelpers.GetNormalizedName(methodCallExpression.Method)}";

        this.ParameterList.AddRange(methodCallExpression.Arguments.Cast<ConstantExpression>().Select(x => x.Value));
        this.CompiledMethod = lambdaExpression.Compile();
    }
}
#endif