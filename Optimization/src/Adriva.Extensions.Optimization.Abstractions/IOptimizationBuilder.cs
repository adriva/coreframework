using System;
using Microsoft.Extensions.DependencyInjection;

namespace Adriva.Extensions.Optimization.Abstractions
{
    public interface IOptimizationBuilder<TOptions> where TOptions : OptimizationOptions
    {
        IServiceCollection Services { get; }

        IOptimizationBuilder<TOptions> AddTransformationChain(string extension, params Type[] transformation);

        IOptimizationBuilder<TOptions> AddFormatter<TFormatter, TType>(int order) where TFormatter : class, IOptimizationResultFormatter<TType>;

    }
}