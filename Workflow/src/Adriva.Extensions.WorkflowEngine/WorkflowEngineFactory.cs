using System;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Adriva.Extensions.WorkflowEngine
{
    public sealed class WorkflowEngineFactory : IWorkflowEngineFactory, IDisposable
    {
        private readonly IServiceProvider ServiceProvider;

        private readonly MemoryCache WorkflowEngineCache = new MemoryCache(new MemoryCacheOptions()
        {
            SizeLimit = 10
        });

        public WorkflowEngineFactory(IServiceProvider serviceProvider)
        {
            this.ServiceProvider = serviceProvider;
        }

        public IWorkflowEngine Get() => this.Get(Options.DefaultName);

        public IWorkflowEngine Get(string name)
        {
            var optionsMonitor = this.ServiceProvider.GetRequiredService<IOptionsMonitor<WorkflowEngineOptions>>();

            int cacheDurationInMinutes = optionsMonitor.Get(name).CompilationCacheDuration;

            if (0 >= cacheDurationInMinutes)
            {
                return new WorkflowEngine(this.ServiceProvider, name, optionsMonitor);
            }
            else
            {
                return this.WorkflowEngineCache.GetOrCreate($"Engine_{name}", entry =>
                {
                    entry.SetSlidingExpiration(TimeSpan.FromMinutes(cacheDurationInMinutes));
                    entry.SetSize(1L);

                    return new WorkflowEngine(this.ServiceProvider, name, optionsMonitor);
                });
            }
        }

        public void Dispose()
        {
            this.WorkflowEngineCache.Dispose();
        }
    }
}
