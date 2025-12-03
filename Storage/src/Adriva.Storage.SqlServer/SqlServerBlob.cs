using System.Diagnostics.CodeAnalysis;
using Adriva.Common.Core;
using Adriva.Storage.Abstractions;

namespace Adriva.Storage.SqlServer;

public record class SqlServerBlob : IBlob
{
    public static readonly SqlServerBlob Empty = new(string.Empty, string.Empty);

    public long Id { get; init; }

    public required string Container { get; init; }

    public required string Name { get; init; }

    public long Size { get; init; }

    [NotNull]
    public string? ETag { get; private set; } = $"\"{DateTimeOffset.UtcNow.Ticks}\"";

    public string MimeType { get; init; }

    public long Properties { get; set; }

    public DateTimeOffset ModifiedOn { get; private set; }

    [SetsRequiredMembers]
    public SqlServerBlob(string container, string name, string? mimeType = null)
    {
        this.Container = container;
        this.Name = Utilities.CompressWhitespaces(Utilities.ConvertToAscii(name));
        this.MimeType = mimeType ?? "application/octet-stream";

        Helpers.CheckName(this.Container, true);
        Helpers.CheckName(this.Name, false);
    }

    [SetsRequiredMembers]
    public SqlServerBlob(long id, string container, string name, string etag, string mimeType, long size, long properties, DateTimeOffset modifiedOn) : this(container, name)
    {
        this.Id = id;
        this.ETag = etag;
        this.MimeType = mimeType;
        this.Size = size;
        this.Properties = properties;
        this.ModifiedOn = modifiedOn;
    }

    public SqlServerBlob<TFlags> WithFlags<TFlags>() where TFlags : struct, Enum
    {
        return new SqlServerBlob<TFlags>(this.Id, this.Container, this.Name, this.ETag, this.MimeType, this.Size, this.Properties, this.ModifiedOn);
    }
}