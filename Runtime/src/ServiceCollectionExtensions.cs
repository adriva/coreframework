using System;
using Adriva.Extensions.Runtime;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    private sealed class ExpressionManagerBuilder : IExpressionManagerBuilder
    {
        public ExpressionManagerBuilder(IServiceCollection services)
        {
            this.Services = services;
            this.Services.AddSingleton<IRuntimeContext, RuntimeContext>();
        }

        public IServiceCollection Services { get; }

        public IExpressionManagerBuilder ConfigureDefaultContext(Action<IRuntimeContext> configure)
        {
            this.Services.Replace(ServiceDescriptor.Singleton<IRuntimeContext, RuntimeContext>(serviceProvider =>
            {
                var instance = ActivatorUtilities.CreateInstance<RuntimeContext>(serviceProvider);
                configure(instance);
                return instance;
            }));
            return this;
        }
    }

    public static IExpressionManagerBuilder AddExpressionManager(this IServiceCollection services)
    {
        services.AddSingleton<IExpressionManager, ExpressionManager>();

        services.AddTransient<IRuntimeTypeResolver, DefaultRuntimeTypeResolver>();

        ExpressionManagerBuilder builder = new(services);
        return builder;
    }
}
