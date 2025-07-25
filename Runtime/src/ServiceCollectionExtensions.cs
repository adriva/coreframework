using Adriva.Extensions.Runtime;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IExpressionManagerBuilder AddExpressionManager(this IServiceCollection services)
    {
        services.AddSingleton<IExpressionManager, ExpressionManager>();

        services.AddTransient<IRuntimeTypeResolver, DefaultRuntimeTypeResolver>();

        ExpressionManagerBuilder builder = new(services);
        return builder;
    }
}
