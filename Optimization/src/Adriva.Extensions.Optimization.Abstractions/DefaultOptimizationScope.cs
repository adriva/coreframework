using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Adriva.Extensions.Optimization.Abstractions
{
    public class DefaultOptimizationScope<TContext> : IOptimizationScope where TContext : class, IOptimizationContext
    {
        private readonly IDictionary<string, IOptimizationContext> Contexts = new Dictionary<string, IOptimizationContext>();
        private readonly IServiceProvider ServiceProvider;

        public IOptimizationContext DefaultContext { get; private set; }

        public DefaultOptimizationScope(IServiceProvider serviceProvider)
        {
            this.ServiceProvider = serviceProvider;

            this.DefaultContext = this.AddOrGetContext(Options.DefaultName);
        }

        public IOptimizationContext AddOrGetContext(string name)
        {
            if (null == name) throw new ArgumentNullException(nameof(name));

            if (this.Contexts.TryGetValue(name, out IOptimizationContext context)) return context;

            IOptimizationContext optimizationContext = ActivatorUtilities.CreateInstance<TContext>(this.ServiceProvider);
            this.Contexts.Add(name, optimizationContext);
            return optimizationContext;
        }

        public async ValueTask DisposeAsync()
        {
            foreach (var entry in this.Contexts)
            {
                if (null != entry.Value)
                {
                    await entry.Value.DisposeAsync();
                }
            }

            this.Contexts.Clear();
        }
    }
}