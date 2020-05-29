using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Adriva.Common.Core
{
    /// <summary>
    /// Represents a wrapper stream that keeps the stream in memory until the memory limit is reached then swaps it with a file stream.
    /// </summary>
    public class AutoStream : Stream
    {
        private readonly long MemoryLimit;

        private SemaphoreSlim SwapLock = new SemaphoreSlim(1, 1);

        private bool HasSwapped = false;

        private Stream InnerStream;

        /// <summary>
        /// Gets the path of the file that is used to store the stream.
        /// </summary>
        /// <remarks>If the stream fits into the memory then this property is null.</remarks>
        /// <value>The path of the file used to store the stream data.</value>
        public string FilePath { get; private set; }

        /// <summary>
        /// Gets a value that indicates whether the current stream supports reading.
        /// </summary>
        public override bool CanRead => this.InnerStream.CanRead;

        /// <summary>
        /// Gets a value that indicates whether the current stream supports seeking.
        /// </summary>
        public override bool CanSeek => this.InnerStream.CanSeek;

        /// <summary>
        /// Gets a value that indicates whether the current stream supports writing.
        /// </summary>
        public override bool CanWrite => this.InnerStream.CanWrite;

        /// <summary>
        /// A long value representing the length of the stream in bytes.
        /// </summary>
        public override long Length => this.InnerStream.Length;

        /// <summary>
        /// Gets or sets the current position of this stream.
        /// </summary>
        /// <value>The current position of this stream.</value>
        public override long Position
        {
            get => this.InnerStream.Position;
            set => this.InnerStream.Position = value;
        }

        /// <summary>
        /// Gets a value that determines whether the current stream can time out.
        /// </summary>
        public override bool CanTimeout => this.InnerStream.CanTimeout;

        /// <summary>
        /// Gets or sets a value, in milliseconds, that determines how long the stream will attempt to read before timing out.
        /// </summary>
        /// <value>A value, in milliseconds, that determines how long the stream will attempt to read before timing out.</value>
        public override int ReadTimeout
        {
            get => this.InnerStream.ReadTimeout;
            set => this.InnerStream.ReadTimeout = value;
        }

        /// <summary>
        /// Gets or sets a value, in milliseconds, that determines how long the stream will attempt to write before timing out.
        /// </summary>
        /// <value>A value, in milliseconds, that determines how long the stream will attempt to write before timing out.</value>
        public override int WriteTimeout
        {
            get => this.InnerStream.WriteTimeout;
            set => this.InnerStream.WriteTimeout = value;
        }

        /// <summary>
        /// Creates a new instance of AutoStream class with optional buffering and a memory limit.
        /// </summary>
        /// <param name="memoryLimit">The maximum number of bytes that can be used in memory before swapping to a FileStream.</param>
        public AutoStream(long memoryLimit)
        {
            this.MemoryLimit = Math.Max(0, memoryLimit);
            this.InnerStream = new MemoryStream();
        }

        private void CheckSwapStream(long requiredByteCount)
        {
            if (!this.HasSwapped && this.MemoryLimit < this.Length + requiredByteCount)
            {
                this.SwapLock.Wait();

                try
                {
                    if (!this.HasSwapped && this.MemoryLimit < this.Length + requiredByteCount)
                    {
                        long position = this.InnerStream.Position;
                        this.FilePath = Path.GetTempFileName();
                        FileStream fileStream = File.Open(this.FilePath, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
                        this.InnerStream.Seek(0, SeekOrigin.Begin);
                        this.InnerStream.CopyTo(fileStream);
                        fileStream.Flush();
                        fileStream.Seek(position, SeekOrigin.Begin);
                        this.InnerStream.Dispose();
                        this.InnerStream = fileStream;
                        this.HasSwapped = true;
                    }
                }
                finally
                {
                    this.SwapLock.Release();
                }
            }
        }

        private async Task CheckSwapStreamAsync(long requiredByteCount)
        {
            if (!this.HasSwapped && this.MemoryLimit < this.Length + requiredByteCount)
            {
                await this.SwapLock.WaitAsync();

                if (!this.HasSwapped && this.MemoryLimit < this.Length + requiredByteCount)
                {
                    try
                    {
                        long position = this.InnerStream.Position;
                        this.FilePath = Path.GetTempFileName();
                        FileStream fileStream = File.Open(this.FilePath, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
                        this.InnerStream.Seek(0, SeekOrigin.Begin);
                        await this.InnerStream.CopyToAsync(fileStream);
                        await fileStream.FlushAsync();
                        await this.InnerStream.DisposeAsync();

                        this.InnerStream = fileStream;

                        this.InnerStream.Seek(position, SeekOrigin.Begin);
                        this.HasSwapped = true;
                    }
                    finally
                    {
                        this.SwapLock.Release();
                    }
                }

            }
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            return this.InnerStream.BeginRead(buffer, offset, count, callback, state);
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            this.CheckSwapStream(count);
            return this.InnerStream.BeginWrite(buffer, offset, count, callback, state);
        }

        public override void Close()
        {
            this.InnerStream.Close();
        }

        public override void CopyTo(Stream destination, int bufferSize)
        {
            this.InnerStream.CopyTo(destination, bufferSize);
        }

        public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
        {
            return this.InnerStream.CopyToAsync(destination, bufferSize, cancellationToken);
        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            return this.InnerStream.EndRead(asyncResult);
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            this.InnerStream.EndWrite(asyncResult);
        }

        public override void Flush()
        {
            this.InnerStream.Flush();
        }

        public override Task FlushAsync(CancellationToken cancellationToken)
        {
            return this.InnerStream.FlushAsync(cancellationToken);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return this.InnerStream.Read(buffer, offset, count);
        }

        public override int Read(Span<byte> buffer)
        {
            return this.InnerStream.Read(buffer);
        }

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return this.InnerStream.ReadAsync(buffer, offset, count, cancellationToken);
        }

        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            return this.InnerStream.ReadAsync(buffer, cancellationToken);
        }

        public override int ReadByte()
        {
            return this.InnerStream.ReadByte();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return this.InnerStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            if (value > this.Length) this.CheckSwapStream(value - this.Length);
            this.InnerStream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            this.CheckSwapStream(count);
            this.InnerStream.Write(buffer, offset, count);
        }

        public override void Write(ReadOnlySpan<byte> buffer)
        {
            this.CheckSwapStream(buffer.Length);
            this.InnerStream.Write(buffer);
        }

        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            await this.CheckSwapStreamAsync(count);
            await this.InnerStream.WriteAsync(buffer, offset, count, cancellationToken);
        }

        public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            await this.CheckSwapStreamAsync(buffer.Length);
            await this.InnerStream.WriteAsync(buffer, cancellationToken);
        }

        public override void WriteByte(byte value)
        {
            this.CheckSwapStream(1);
            this.InnerStream.WriteByte(value);
        }

        private void DeleteTempFile()
        {
            if (!this.HasSwapped || string.IsNullOrWhiteSpace(this.FilePath)) return;

            try
            {
                File.Delete(this.FilePath);
            }
            finally
            {

            }
        }

        public override async ValueTask DisposeAsync()
        {
            this.SwapLock.Dispose();
            await this.InnerStream.DisposeAsync();
            this.DeleteTempFile();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.SwapLock.Dispose();
                this.InnerStream.Dispose();
            }

            this.DeleteTempFile();
        }

        public override string ToString()
        {
            return $"AutoStream";
        }
    }
}