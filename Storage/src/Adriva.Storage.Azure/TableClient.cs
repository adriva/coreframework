using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Adriva.Common.Core;
using Adriva.Storage.Abstractions;
using Azure.Data.Tables;
using Microsoft.Extensions.DependencyInjection;
using AzureTableClient = Azure.Data.Tables.TableClient;
using MsAzure = Azure;

namespace Adriva.Storage.Azure
{
    public class TableClient : ITableClient, IAsyncInitializedStorageClient<AzureTableClientOptions>
    {
        private readonly IServiceProvider ServiceProvider;
        private readonly ITableEntityMapperFactory TableEntityMapperFactory;

        protected AzureTableClient Table { get; private set; }

        protected AzureTableClientOptions Options { get; private set; }

        private ITableEntity Convert<TItem>(TItem item) where TItem : class
        {
            if (item is ITableEntity itemTableEntity)
            {
                return itemTableEntity;
            }
            else
            {
                var mapper = this.TableEntityMapperFactory.GetMapper<TItem>();
                return mapper.Build(item);
            }
        }

        public TableClient(IServiceProvider serviceProvider)
        {
            this.ServiceProvider = serviceProvider;
            this.TableEntityMapperFactory = this.ServiceProvider.GetRequiredService<ITableEntityMapperFactory>();
        }

        public async ValueTask InitializeAsync(AzureTableClientOptions options)
        {
            this.Options = options;
            var tableServiceClient = new TableServiceClient(this.Options.ConnectionString);
            await tableServiceClient.CreateTableIfNotExistsAsync(this.Options.TableName);
            this.Table = tableServiceClient.GetTableClient(this.Options.TableName);

        }

        protected virtual async Task ExecuteBatchAsync(IEnumerable<TableTransactionAction> transactionActions, int batchSize = 100)
        {
            const int MaxItemsInTransaction = 100;
            batchSize = Math.Max(1, Math.Min(MaxItemsInTransaction, batchSize));
            int pageIndex = 0;
            bool hasMore = true;

            do
            {
                var selectedTransactionActions = transactionActions.Skip(pageIndex * batchSize).Take(batchSize);

                if (selectedTransactionActions.Any())
                {
                    await this.Table.SubmitTransactionAsync(selectedTransactionActions);
                    ++pageIndex;
                }
                else
                {
                    hasMore = false;
                }
            } while (hasMore);
        }

        public async Task BatchDeleteAsync(string partitionKey)
        {
            if (string.IsNullOrWhiteSpace(partitionKey))
            {
                return;
            }

            int rowCount = 500;
            string continuationToken = null;
            var pagedData = this.Table.QueryAsync<TableEntity>($"PartitionKey eq '{partitionKey}'", rowCount, new string[] { nameof(TableEntity.PartitionKey), nameof(TableEntity.RowKey) });

            await foreach (var page in pagedData.AsPages(continuationToken, rowCount))
            {
                await this.ExecuteBatchAsync(page.Values.Select(x => new TableTransactionAction(TableTransactionActionType.Delete, x)));
            }
        }

        public Task BatchDeleteAsync<TItem>(IEnumerable<TItem> items, bool forceDelete = false, int batchSize = 100) where TItem : class
        {
            return this.ExecuteBatchAsync(items.Select(x => new TableTransactionAction(TableTransactionActionType.Delete, this.Convert(x))), batchSize);
        }

        public Task BatchUpsertAsync<TItem>(IEnumerable<TItem> items, int batchSize = 100) where TItem : class
        {
            return this.ExecuteBatchAsync(items.Select(x => new TableTransactionAction(TableTransactionActionType.UpsertReplace, this.Convert(x))), batchSize);
        }

        public async Task DeleteAsync(string partitionKey, string rowKey)
        {
            await this.Table.DeleteEntityAsync(partitionKey, rowKey, MsAzure.ETag.All);
        }

        public async Task<TItem> GetAsync<TItem>(string partitionKey, string rowKey) where TItem : class
        {
            var response = await this.Table.GetEntityIfExistsAsync<TableEntity>(partitionKey, rowKey);

            if (!response.HasValue)
            {
                return null;
            }

            var mapper = this.TableEntityMapperFactory.GetMapper<TItem>();
            return mapper.Build(response.Value);
        }

        public async Task<SegmentedResult<TItem>> SelectAsync<TItem>(FormattableString formattableQuery, string continuationToken = null, int rowCount = 500) where TItem : class
        {
            if (string.IsNullOrWhiteSpace(formattableQuery?.Format))
            {
                throw new ArgumentNullException(nameof(formattableQuery));
            }

            string query = AzureTableClient.CreateQueryFilter(formattableQuery);

            var pagedData = this.Table.QueryAsync<TableEntity>(query, rowCount).AsPages(continuationToken, rowCount);

            var mapper = this.TableEntityMapperFactory.GetMapper<TItem>();

            await foreach (var page in pagedData)
            {
                return new SegmentedResult<TItem>(page.Values.Select(x => mapper.Build(x)), page.ContinuationToken, !string.IsNullOrWhiteSpace(page.ContinuationToken));
            }

            return SegmentedResult<TItem>.Empty;
        }

        public async Task<SegmentedResult<TItem>> SelectAsync<TItem>(Expression<Func<TItem, bool>> queryExpression, string continuationToken = null, int rowCount = 500) where TItem : class
        {
            AzureTableQueryNormalizer normalizer = new AzureTableQueryNormalizer();

            Expression<Func<TableEntity, bool>> normalizedQueryExpression = (Expression<Func<TableEntity, bool>>)normalizer.Visit(queryExpression);

            var query = AzureTableClient.CreateQueryFilter(normalizedQueryExpression);

            var pagedData = this.Table.QueryAsync<TableEntity>(query, rowCount).AsPages(continuationToken, rowCount);

            var mapper = this.TableEntityMapperFactory.GetMapper<TItem>();

            await foreach (var page in pagedData)
            {
                return new SegmentedResult<TItem>(page.Values.Select(x => mapper.Build(x)), page.ContinuationToken, !string.IsNullOrWhiteSpace(page.ContinuationToken));
            }

            return SegmentedResult<TItem>.Empty;
        }

        public async Task UpdateAsync<TItem>(TItem item, bool forceUpdate = false) where TItem : class
        {
            if (null == item)
            {
                return;
            }

            ITableEntity tableEntity = this.Convert(item);

            var eTag = MsAzure.ETag.All;

            if (!forceUpdate && !tableEntity.ETag.Equals(null))
            {
                eTag = tableEntity.ETag;
            }

            try
            {
                await this.Table.UpdateEntityAsync(tableEntity, eTag, TableUpdateMode.Merge);
            }
            catch (MsAzure.RequestFailedException requestFailedException)
            {
                if (412 == requestFailedException.Status)
                {
                    throw new OptimisticConcurrencyException("The record you are trying to update has changed.", requestFailedException);
                }
                else throw;
            }
        }

        public async Task UpsertAsync<TItem>(TItem item) where TItem : class
        {
            if (null == item)
            {
                return;
            }

            var eTag = MsAzure.ETag.All;

            ITableEntity tableEntity = this.Convert(item);

            await this.Table.UpsertEntityAsync(tableEntity, TableUpdateMode.Replace);
        }
    }
}
