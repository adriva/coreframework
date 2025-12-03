using System;
using Microsoft.Extensions.DependencyInjection;

namespace Adriva.Extensions.Reporting.Abstractions
{
    public class ReportContext : IDisposable
    {
        private bool IsDisposed;

        public ReportDefinition ReportDefinition { get; private set; }

        public object ContextProvider { get; private set; }

        public PostProcessor PostProcessor { get; private set; }

        public static ReportContext Create(IServiceProvider serviceProvider, ReportDefinition reportDefinition)
        {
            return new ReportContext(serviceProvider, reportDefinition);
        }

        protected ReportContext(IServiceProvider serviceProvider, ReportDefinition reportDefinition)
        {
            if (null == serviceProvider) throw new ArgumentNullException(nameof(serviceProvider));
            this.ReportDefinition = reportDefinition ?? throw new ArgumentNullException(nameof(reportDefinition));

            if (!string.IsNullOrWhiteSpace(reportDefinition.ContextProvider))
            {
                Type contextProviderType = Type.GetType(reportDefinition.ContextProvider, true, true);
                this.ContextProvider = ActivatorUtilities.CreateInstance(serviceProvider, contextProviderType);
            }

            if (!string.IsNullOrWhiteSpace(reportDefinition.PostProcessor))
            {
                Type postProcessorType = Type.GetType(reportDefinition.PostProcessor, true, true);
                this.PostProcessor = (PostProcessor)ActivatorUtilities.CreateInstance(serviceProvider, postProcessorType);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.IsDisposed)
            {
                if (disposing)
                {
                    if (this.ContextProvider is IDisposable disposableContextProvider)
                    {
                        disposableContextProvider.Dispose();
                        this.ContextProvider = null;
                    }

                    if (this.PostProcessor is IDisposable disposablePostProcessor)
                    {
                        disposablePostProcessor.Dispose();
                        this.PostProcessor = null;
                    }
                }

                this.IsDisposed = true;
            }
        }

        public void Dispose()
        {
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}