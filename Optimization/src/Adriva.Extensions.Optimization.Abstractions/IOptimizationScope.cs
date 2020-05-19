using System;

namespace Adriva.Extensions.Optimization.Abstractions
{
    public interface IOptimizationScope : IDisposable
    {
        IOptimizationContext DefaultContext { get; }

        IOptimizationContext AddOrGetContext(string name);
    }
}