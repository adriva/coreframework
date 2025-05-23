using System;
using Microsoft.Extensions.DependencyInjection;

namespace Adriva.Extensions.Runtime;

public interface IExpressionManagerBuilder
{
    IServiceCollection Services { get; }

    IExpressionManagerBuilder ConfigureDefaultContext(Action<IRuntimeContext> configure);
}
