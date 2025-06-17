using Adriva.Extensions.Runtime;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

public static class ExpressionManagerBuilderExtensions
{
    public static IExpressionManagerBuilder AddTypeResolver<T>(this IExpressionManagerBuilder builder) where T : class, IRuntimeTypeResolver
    {
        builder.Services.Replace(ServiceDescriptor.Transient<IRuntimeTypeResolver, T>());
        return builder;
    }
}
