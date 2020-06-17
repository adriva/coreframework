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

        public async Task<TItem> GetAsync<TItem>(string partitionKey, string rowKey) where TItem : class, ITableItem, new()
        {
            TableOperation retrieveOperation = TableOperation.Retrieve(partitionKey, rowKey);
            var tableResult = await this.Table.ExecuteAsync(retrieveOperation);
            return this.Assembler.Assemble<TItem>(tableResult.Result as DynamicTableEntity);
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

            IEnumerable<TItem> itemsList = azureResult.Results.Select(x => this.Assembler.Assemble<TItem>(x as DynamicTableEntity));
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

            this.Logger.LogInformation($"Azure Table Query: {query.FilterString}");

            var token = AzureStorageUtilities.DeserializeTableContinuationToken(continuationToken);
            var azureResult = await this.Table.ExecuteQuerySegmentedAsync(query, token);

            IEnumerable<TItem> itemsList = azureResult.Results.Select(x => this.Assembler.Assemble<TItem>(x as DynamicTableEntity));
            string nextPageToken = azureResult.ContinuationToken.Serialize();
            return new SegmentedResult<TItem>(itemsList, nextPageToken, null != nextPageToken);
        }

        public async Task UpsertAsync<TItem>(TItem item) where TItem : class, ITableItem
        {
            if (null == item) throw new ArgumentNullException(nameof(item));

            var entity = this.Assembler.Disassemble(item);
            var tableOperation = TableOperation.InsertOrReplace(entity);
            await this.Table.ExecuteAsync(tableOperation);
        }

        public ValueTask DisposeAsync()
        {
            this.Table = null;
            return new ValueTask();
        }
    }
}