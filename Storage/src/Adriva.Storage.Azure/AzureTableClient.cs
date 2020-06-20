using System.IO;
using System.Threading.Tasks;
using Adriva.Storage.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Azure.Cosmos.Table;
using Adriva.Common.Core;
using System.Linq;
using System.Linq.Expressions;
using System;
using System.Collections.Generic;
using Microsoft.OData.Client;
using Microsoft.Extensions.Logging;
using System.Web;

namespace Adriva.Storage.Azure
{
    public sealed class AzureTableClient : ITableClient
    {
        private readonly ILogger Logger;
        private readonly ITableItemAssembler Assembler;
        private readonly IOptionsMonitor<AzureTableConfiguration> ConfigurationAccessor;
        private AzureTableConfiguration Configuration;
        private CloudTable Table;

        public AzureTableClient(IOptionsMonitor<AzureTableConfiguration> configurationAccessor, ITableItemAssembler builder, ILogger<AzureTableClient> logger)
        {
            this.Logger = logger;
            this.Assembler = builder;
            this.ConfigurationAccessor = configurationAccessor;
        }

        public async ValueTask InitializeAsync(string clientName)
        {
            this.Configuration = this.ConfigurationAccessor.Get(clientName);

            if (!CloudStorageAccount.TryParse(this.Configuration.ConnectionString, out CloudStorageAccount account))
            {
                throw new InvalidDataException($"Azure blob connection string for blob client '{clientName}' could not be parsed.");
            }

            var cloudTableClient = account.CreateCloudTableClient();
            this.Table = cloudTableClient.GetTableReference(this.Configuration.TableName);
            await this.Table.CreateIfNotExistsAsync();
        }

        private async Task BatchExecuteAsync<TItem>(IEnumerable<TItem> items, int batchSize, Action<TableBatchOperation, ITableEntity> batchStepCallback) where TItem : class, ITableItem
        {
            if (null == items) throw new ArgumentNullException(nameof(items));

            batchSize = Math.Min(100, batchSize);

            var partitionedItems = items.GroupBy(x => x.PartitionKey);

            foreach (var partitionedItem in partitionedItems)
            {
                int pageIndex = 0;

                var deleteItems = partitionedItem.Skip(pageIndex * batchSize).Take(batchSize);

                if (!deleteItems.Any()) continue;

                TableBatchOperation batchOperation = new TableBatchOperation();
                foreach (var deleteItem in deleteItems)
                {
                    var tableEntity = this.Assembler.Disassemble(deleteItem);
                    batchStepCallback.Invoke(batchOperation, tableEntity);
                }

                ++pageIndex;
                await this.Table.ExecuteBatchAsync(batchOperation);
            }
        }

        public async Task<TItem> GetAsync<TItem>(string partitionKey, string rowKey) where TItem : class, ITableItem, new()
        {
            TableOperation retrieveOperation = TableOperation.Retrieve(partitionKey, rowKey);
            var tableResult = await this.Table.ExecuteAsync(retrieveOperation);
            return this.Assembler.Assemble<TItem>((DynamicTableEntity)tableResult.Result);
        }

        public async Task<SegmentedResult<TItem>> GetAllAsync<TItem>(string continuationToken = null, string partitionKey = null, string rowKey = null, int rowCount = 500) where TItem : class, ITableItem, new()
        {
            string partitionQuery = null, rowQuery = null;

            TableQuery query = new TableQuery();

            if (!string.IsNullOrWhiteSpace(partitionKey)) partitionQuery = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey);
            if (!string.IsNullOrWhiteSpace(rowKey)) rowQuery = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, rowKey);

            if (null != partitionQuery) query = query.Where(partitionQuery);
            if (null != rowQuery) query = query.Where(rowQuery);

            query.TakeCount = rowCount;

            var token = AzureStorageUtilities.DeserializeTableContinuationToken(continuationToken);
            var azureResult = await this.Table.ExecuteQuerySegmentedAsync(query, token);

            IEnumerable<TItem> itemsList = azureResult.Results.Select(x => this.Assembler.Assemble<TItem>((DynamicTableEntity)x));
            string nextPageToken = azureResult.ContinuationToken.Serialize();
            return new SegmentedResult<TItem>(itemsList, nextPageToken, null != nextPageToken);
        }

        public async Task<SegmentedResult<TItem>> SelectAsync<TItem>(Expression<Func<TItem, bool>> queryExpression, string continuationToken = null, int rowCount = 500) where TItem : class, ITableItem, new()
        {
            QueryExpressionVisitor<TItem> visitor = new QueryExpressionVisitor<TItem>();
            _ = (Expression<Func<TItem, bool>>)visitor.Visit(queryExpression);

            DataServiceContext context = new DataServiceContext(new Uri("https://tempuri.org"));
            var dataServiceQuery = context.CreateQuery<TItem>("/items").Where(queryExpression);
            string queryString = (((DataServiceQuery)dataServiceQuery).RequestUri).Query;
            var queryCollection = HttpUtility.ParseQueryString(queryString);

            TableQuery query = new TableQuery()
            {
                FilterString = queryCollection.Get("$filter"),
                TakeCount = rowCount,
            };

            this.Logger.LogInformation($"Running Azure table query: {query.FilterString}");

            var token = AzureStorageUtilities.DeserializeTableContinuationToken(continuationToken);
            var azureResult = await this.Table.ExecuteQuerySegmentedAsync(query, token);

            IEnumerable<TItem> itemsList = azureResult.Results.Select(x => this.Assembler.Assemble<TItem>((DynamicTableEntity)x));
            string nextPageToken = azureResult.ContinuationToken.Serialize();
            return new SegmentedResult<TItem>(itemsList, nextPageToken, null != nextPageToken);
        }

        public async Task UpsertAsync<TItem>(TItem item) where TItem : class, ITableItem
        {
            if (null == item) throw new ArgumentNullException(nameof(item));

            var entity = this.Assembler.Disassemble(item);
            var tableOperation = TableOperation.InsertOrReplace(entity);
            this.Logger.LogInformation($"Upserting entity with PartitionKey = '{entity.PartitionKey}' and RowKey = '{entity.RowKey}'.");
            await this.Table.ExecuteAsync(tableOperation);
            this.Logger.LogInformation($"Upserted entity with PartitionKey = '{entity.PartitionKey}' and RowKey = '{entity.RowKey}'.");
        }

        public async Task UpdateAsync<TItem>(TItem item, string etag = "*") where TItem : class, ITableItem
        {
            if (null == item) throw new ArgumentNullException(nameof(item));

            var tableEntity = this.Assembler.Disassemble(item);
            tableEntity.ETag = etag;

            TableOperation updateOperation = TableOperation.Replace(tableEntity);
            await this.Table.ExecuteAsync(updateOperation);
        }

        public async Task BatchUpsertAsync<TItem>(IEnumerable<TItem> items, int batchSize = 100) where TItem : class, ITableItem
        {
            await this.BatchExecuteAsync(items, batchSize, (operation, entity) =>
            {
                operation.InsertOrReplace(entity);
            });
        }

        public async Task DeleteAsync(string partitionKey, string rowKey, string etag = "*")
        {
            DynamicTableEntity dynamicTableEntity = new DynamicTableEntity(partitionKey, rowKey);
            dynamicTableEntity.ETag = etag;
            TableOperation deleteOperation = TableOperation.Delete(dynamicTableEntity);
            await this.Table.ExecuteAsync(deleteOperation);
        }

        public async Task BatchDeleteAsync<TItem>(IEnumerable<TItem> items, bool forceDelete = false, int batchSize = 100) where TItem : class, ITableItem
        {
            await this.BatchExecuteAsync(items, batchSize, (operation, entity) =>
            {
                if (forceDelete) entity.ETag = "*";
                operation.Delete(entity);
            });
        }

        public ValueTask DisposeAsync()
        {
            this.Table = null;
            return new ValueTask();
        }
    }
}