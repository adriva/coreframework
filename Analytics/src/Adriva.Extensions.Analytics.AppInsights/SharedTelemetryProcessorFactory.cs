using System;
using Microsoft.ApplicationInsights.Extensibility;

namespace Microsoft.Extensions.DependencyInjection
{
    internal class SharedTelemetryProcessorFactory : ApplicationInsights.AspNetCore.ITelemetryProcessorFactory, ApplicationInsights.WorkerService.ITelemetryProcessorFactory
    {
        private readonly IServiceProvider serviceProvider;
        private readonly Type telemetryProcessorType;

        /// <summary>
        /// Initializes a new instance of the <see cref="TelemetryProcessorFactory"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="telemetryProcessorType">The type of telemetry processor to create.</param>
        public SharedTelemetryProcessorFactory(IServiceProvider serviceProvider, Type telemetryProcessorType)
        {
            this.serviceProvider = serviceProvider;
            this.telemetryProcessorType = telemetryProcessorType;
        }

        /// <inheritdoc />
        public ITelemetryProcessor Create(ITelemetryProcessor next)
        {
            return (ITelemetryProcessor)ActivatorUtilities.CreateInstance(this.serviceProvider, this.telemetryProcessorType, next);
        }
    }
}
