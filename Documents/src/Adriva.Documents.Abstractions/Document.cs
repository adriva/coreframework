using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Adriva.Documents.Abstractions
{

    public abstract class Document : IDocument
    {
        private bool IsDisposed;

        public abstract bool CanSave { get; }

        protected abstract Task SaveDocumentAsync(Stream stream);

        public abstract IEnumerable<T> GetParts<T>() where T : IDocumentPart;

        public abstract T AddPart<T>() where T : class, IDocumentPart;

        public Task SaveAsync(Stream stream)
        {
            if (!this.CanSave)
            {
                throw new InvalidOperationException("This document cannot be saved.");
            }

            return this.SaveDocumentAsync(stream);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.IsDisposed)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                this.IsDisposed = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~SpreahsheetDocument()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}