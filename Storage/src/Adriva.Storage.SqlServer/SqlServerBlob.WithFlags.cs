using System.Diagnostics.CodeAnalysis;
using Adriva.Common.Core;
using Adriva.Storage.Abstractions;

namespace Adriva.Storage.SqlServer;

public record class SqlServerBlob<TFlags> : SqlServerBlob where TFlags : struct, Enum
{
    public TFlags Flags
    {
        get => (TFlags)Enum.ToObject(typeof(TFlags), this.Properties);
        set => this.Properties = Convert.ToInt64(value);
    }

    [SetsRequiredMembers]
    public SqlServerBlob(string container, string name, string? mimeType = null)
        : base(container, name, mimeType)
    {

    }

    [SetsRequiredMembers]
    public SqlServerBlob(long id, string container, string name, string etag, string mimeType, long size, long properties, DateTimeOffset modifiedOn)
        : base(id, container, name, etag, mimeType, size, properties, modifiedOn)
    {

    }
}
