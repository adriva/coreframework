using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Adriva.Common.Core;

namespace Adriva.Storage.Abstractions
{
    public interface ITableClient : IStorageClient
    {
        Task<TItem> GetAsync<TItem>(string partitionKey, string rowKey) where TItem : class;

        Task<SegmentedResult<TItem>> SelectAsync<TItem>(Expression<Func<TItem, bool>> queryExpression, string continuationToken = null, int rowCount = 500) where TItem : class;

        Task UpsertAsync<TItem>(TItem item) where TItem : class;

        Task UpdateAsync<TItem>(TItem item, bool forceUpdate = false) where TItem : class;

        Task BatchUpsertAsync<TItem>(IEnumerable<TItem> items, int batchSize = 100) where TItem : class;

        Task DeleteAsync(string partitionKey, string rowKey);

        Task BatchDeleteAsync<TItem>(IEnumerable<TItem> items, bool forceDelete = false, int batchSize = 100) where TItem : class;
    }
}