namespace Adriva.Storage.Abstractions;

public interface IBlob
{
    string Container { get; }

    string Name { get; }

    string? ETag { get; }

    string? MimeType { get; }

    DateTimeOffset ModifiedOn { get; }
}
