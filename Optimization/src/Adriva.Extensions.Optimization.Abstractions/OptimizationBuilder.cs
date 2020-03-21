using System;
using Microsoft.Extensions.DependencyInjection;

namespace Adriva.Extensions.Optimization.Abstractions
{
    internal sealed class OptimizationBuilder<TOptions> : IOptimizationBuilder<TOptions> where TOptions : OptimizationOptions
    {
        public IServiceCollection Services { get; private set; }

        public OptimizationBuilder(IServiceCollection services)
        {
            this.Services = services;
        }

        public IOptimizationBuilder<TOptions> AddTransformationChain(string extension, params Type[] transforms)
        {
            this.Services.ConfigureAll<TOptions>(options =>
            {
                if (!extension.StartsWith("."))
                {
                    extension = "." + extension;
                }

                options.AddTransformChain(extension, transforms);
            });
            return this;
        }

        public IOptimizationBuilder<TOptions> AddFormatter<TFormatter, TType>(int order) where TFormatter : class, IOptimizationResultFormatter<TType>
        {
            this.Services.ConfigureAll<TOptions>(options =>
            {
                options.AddFormatter(order, typeof(TFormatter));
            });
            return this;
        }
    }
}