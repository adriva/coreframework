using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace Adriva.Extensions.Optimization.Abstractions
{
    public class DefaultOptimizationScope : IOptimizationScope
    {
        private readonly IDictionary<string, IOptimizationContext> Contexts = new Dictionary<string, IOptimizationContext>();
        private readonly IServiceProvider ServiceProvider;

        public IOptimizationContext DefaultContext { get; private set; }

        public DefaultOptimizationScope(IServiceProvider serviceProvider)
        {
            this.ServiceProvider = serviceProvider;

            this.DefaultContext = this.AddOrGetContext("Default");
        }

        public IOptimizationContext AddOrGetContext(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));

            if (this.Contexts.TryGetValue(name, out IOptimizationContext context)) return context;

            IOptimizationContext optimizationContext = this.ServiceProvider.GetRequiredService<IOptimizationContext>();
            this.Contexts.Add(name, optimizationContext);
            return optimizationContext;
        }

        public void Dispose()
        {
            foreach (var entry in this.Contexts)
            {
                entry.Value?.Dispose();
            }

            this.Contexts.Clear();
        }
    }
}