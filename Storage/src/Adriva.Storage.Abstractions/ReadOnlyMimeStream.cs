namespace Adriva.Storage.Abstractions;

public class ReadOnlyMimeStream(Stream stream, string? mimeType) : Stream
{
    private readonly BufferedStream BufferedStream = new(stream);

    public string MimeType { get; private set; } = string.IsNullOrWhiteSpace(mimeType) ? "application/octet-stream" : mimeType;

    public override bool CanSeek => this.BufferedStream.CanSeek;

    public override bool CanWrite => false;

    public override bool CanRead => this.BufferedStream.CanRead;

    public override long Position
    {
        get => this.BufferedStream.Position;
        set => this.BufferedStream.Seek(value, SeekOrigin.Begin);
    }

    public override long Length => this.BufferedStream.Length;

    public override void SetLength(long value)
    {
        // has no effect
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        return this.BufferedStream.Seek(offset, origin);
    }

    public override void Flush()
    {
        this.BufferedStream.Flush();
    }

    public override Task FlushAsync(CancellationToken cancellationToken)
    {
        return this.BufferedStream.FlushAsync(cancellationToken);
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        return this.BufferedStream.Read(buffer, offset, count);
    }

    public override sealed void Write(byte[] buffer, int offset, int count)
    {
        throw new NotSupportedException($"{nameof(ReadOnlyMimeStream)} is read-only and doesn't support this method.");
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            this.BufferedStream.Dispose();
        }
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        await this.BufferedStream.DisposeAsync();
        GC.SuppressFinalize(this);
    }
}