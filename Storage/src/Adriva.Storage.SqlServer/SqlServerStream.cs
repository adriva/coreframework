using System;
using System.Data.Common;
using System.IO;
using System.Threading.Tasks;

namespace Adriva.Storage.SqlServer
{
    internal sealed class SqlServerStream : Stream
    {
        private readonly Stream InnerStream;
        private readonly DbDataReader Reader;
        private readonly DbCommand Command;
        private readonly DbConnection Connection;

        public static readonly SqlServerStream Empty = new SqlServerStream(new MemoryStream(Array.Empty<byte>()), null, null, null);

        public override bool CanRead => this.InnerStream.CanRead;

        public override bool CanSeek => this.InnerStream.CanSeek;

        public override bool CanWrite => this.InnerStream.CanWrite;

        public override long Length => this.InnerStream.Length;

        public override long Position { get => this.InnerStream.Position; set => this.InnerStream.Position = value; }

        public SqlServerStream(Stream innerStream, DbDataReader reader, DbCommand command, DbConnection connection)
        {
            this.InnerStream = innerStream;
            this.Reader = reader;
            this.Command = command;
            this.Connection = connection;
        }

        public override void Flush()
        {
            this.InnerStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return this.InnerStream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return this.InnerStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            this.InnerStream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            this.InnerStream.Write(buffer, offset, count);
        }

        public override void Close()
        {
            if (null != this.InnerStream) this.InnerStream.Close();
            if (null != this.Reader) this.Reader.Close();
            if (null != this.Connection) this.Connection.Close();
            base.Close();
        }

        protected override void Dispose(bool disposing)
        {
            if (null != this.InnerStream) this.InnerStream.Dispose();
            if (null != this.Reader) this.Reader.Dispose();
            if (null != this.Command) this.Command.Dispose();
            if (null != this.Connection) this.Connection.Dispose();
            base.Dispose(disposing);
        }

        public override async ValueTask DisposeAsync()
        {
            if (null != this.InnerStream) await this.InnerStream.DisposeAsync();
            if (null != this.Reader) await this.Reader.DisposeAsync();
            if (null != this.Command) await this.Command.DisposeAsync();
            if (null != this.Connection) await this.Connection.DisposeAsync();
            await base.DisposeAsync();
        }
    }
}