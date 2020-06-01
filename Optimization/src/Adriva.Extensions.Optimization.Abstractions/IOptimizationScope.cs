using System;

namespace Adriva.Extensions.Optimization.Abstractions
{
    public interface IOptimizationScope : IAsyncDisposable
    {
        IOptimizationContext DefaultContext { get; }

        IOptimizationContext AddOrGetContext(string name);
    }
}