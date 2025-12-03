using System;
using Adriva.Extensions.WorkflowEngine;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class WorkflowServiceExtensions
    {
        public static IServiceCollection AddWorkflowEngine(this IServiceCollection services, Action<WorkflowEngineOptions> configure)
        {
            return services.AddWorkflowEngine(Options.Options.DefaultName, configure);
        }

        public static IServiceCollection AddWorkflowEngine(this IServiceCollection services, string name, Action<WorkflowEngineOptions> configure)
        {
            services.AddSingleton<IWorkflowEngineFactory, WorkflowEngineFactory>();
            services.Configure(name, configure);
            return services;
        }
    }
}
