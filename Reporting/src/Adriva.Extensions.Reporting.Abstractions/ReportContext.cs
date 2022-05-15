using System;

namespace Adriva.Extensions.Reporting.Abstractions
{
    public class ReportContext : IDisposable
    {
        private bool IsDisposed;

        public ReportDefinition ReportDefinition { get; private set; }

        public object ContextProvider { get; internal set; }

        public PostProcessor PostProcessor { get; internal set; }

        public ReportContext(ReportDefinition reportDefinition)
        {
            this.ReportDefinition = reportDefinition ?? throw new ArgumentNullException(nameof(reportDefinition));
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