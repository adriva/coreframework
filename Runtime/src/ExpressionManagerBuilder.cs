using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Adriva.Extensions.Runtime;

internal sealed class ExpressionManagerBuilder : IExpressionManagerBuilder
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
