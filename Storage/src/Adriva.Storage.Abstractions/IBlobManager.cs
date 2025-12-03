using System.Linq.Expressions;
using Adriva.Common.Core;

namespace Adriva.Storage.Abstractions;

public interface IBlobManager<T> where T : class, IBlob
{
    Task<SegmentedResult<T>> ListAsync(Expression<Func<T, bool>> predicate, string? continuationToken = null, int segmentSize = 100);

    Task<T?> GetAsync(Expression<Func<T, bool>> predicate);

    Task<T> UpsertAsync(T blob, Stream sourceStream);

    Task DeleteAsync(Expression<Func<T, bool>> predicate);

    ValueTask<bool> ExistsAsync(Expression<Func<T, bool>> predicate);

    Task ProcessReadStreamAsync(Expression<Func<T, bool>> predicate, Func<Stream, string?, Task> streamProcessor);

    Task<ReadOnlyMimeStream> OpenReadStreamAsync(Expression<Func<T, bool>> predicate);

}
