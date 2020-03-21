using System;

namespace Adriva.Extensions.Optimization.Abstractions
{
    public interface IOptimizationEvents<TOptions> where TOptions : OptimizationOptions
    {
        Action<IServiceProvider, TOptions> ServiceInitialized { get; }

        Func<string, TOptions, ITransform, bool> TransformRunning { get; }

    }
}