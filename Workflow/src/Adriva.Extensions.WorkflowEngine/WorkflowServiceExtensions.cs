using System;
using Adriva.Extensions.WorkflowEngine;
#if !NET7COMPILER
using Microsoft.Extensions.Options;
#endif

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
#if NET7COMPILER
            services.Configure(name, configure);
#else
            services.AddSingleton<IConfigureOptions<WorkflowEngineOptions>>(new ConfigureNamedOptions<WorkflowEngineOptions>(name, configure));
#endif
            return services;
        }
    }
}
