using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Adriva.Analytics.Abstractions
{
    internal sealed class FileTransaction : IDisposable
    {
        private readonly string FilePath;
        private readonly FileStream Stream;

        private bool IsDisposed = false;
        private int IsTransactionCommitted = 0;

        public static FileTransaction Create(string path, FileMode mode, FileAccess access = FileAccess.Write, FileShare share = FileShare.Read)
        {
            return new FileTransaction(path, mode, access, share);
        }

        private FileTransaction(string path, FileMode mode, FileAccess access = FileAccess.Write, FileShare share = FileShare.Read)
        {
            this.Stream = File.Open(path, mode, access, share);
            this.FilePath = path;
        }

        public async Task WriteAsync(byte[] buffer)
        {
            if (this.IsDisposed)
            {
                throw new ObjectDisposedException("FileTransaction");
            }

            await this.Stream.WriteAsync(buffer, 0, buffer.Length);
        }

        public void Commit()
        {
            if (this.IsDisposed)
            {
                throw new ObjectDisposedException("FileTransaction");
            }

            if (0 == Interlocked.CompareExchange(ref this.IsTransactionCommitted, 1, 0))
            {
                File.Delete(this.FilePath);
            }
            else
            {
                throw new InvalidProgramException($"Transaction on file '{this.FilePath}' has already been committed.");
            }
        }

        #region IDisposable

        public void Dispose()
        {
            if (!this.IsDisposed)
            {
                this.Stream?.Flush();
                this.Stream?.Dispose();
                this.IsDisposed = true;
            }
            else
            {
                throw new ObjectDisposedException("FileTransaction");
            }
        }
        #endregion
    }
}
