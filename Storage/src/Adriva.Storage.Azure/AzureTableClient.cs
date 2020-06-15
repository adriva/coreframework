using System.IO;
using System.Threading.Tasks;
using Adriva.Storage.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Azure.Cosmos.Table;
using System.Diagnostics;
using Adriva.Common.Core;
using System.Linq;
using System.Linq.Expressions;
using System;
using System.Collections.Generic;

namespace Adriva.Storage.Azure
{
    public sealed class AzureTableClient : ITableClient
    {
        private static readonly TableEntityBuilder Builder = new TableEntityBuilder();
        private readonly IOptionsMonitor<AzureTableConfiguration> ConfigurationAccessor;
        private AzureTableConfiguration Configuration;
        private CloudTable Table;

        public AzureTableClient(IOptionsMonitor<AzureTableConfiguration> configurationAccessor)
        {
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

        public ValueTask DisposeAsync()
        {
            this.Table = null;
            return new ValueTask();
        }

        public async Task<TItem> GetAsync<TItem>(string partitionKey, string rowKey) where TItem : class, new()
        {
            TableOperation retrieveOperation = TableOperation.Retrieve(partitionKey, rowKey);
            var tableResult = await this.Table.ExecuteAsync(retrieveOperation);
            return AzureTableClient.Builder.Build<TItem>(tableResult.Result as DynamicTableEntity);
        }

        public async Task<SegmentedResult<TItem>> GetAllAsync<TItem>(string continuationToken = null, string partitionKey = null, string rowKey = null, int rowCount = 500) where TItem : class, new()
        {
            string partitionQuery = null, rowQuery = null;

            TableQuery query = new TableQuery();

            if (!string.IsNullOrWhiteSpace(partitionKey)) partitionQuery = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey);
            if (!string.IsNullOrWhiteSpace(rowKey)) rowQuery = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, rowKey);

            if (null != partitionQuery) query = query.Where(partitionQuery);
            if (null != rowQuery) query = query.Where(rowQuery);

            var token = AzureStorageUtilities.DeserializeTableContinuationToken(continuationToken);
            var azureResult = await this.Table.ExecuteQuerySegmentedAsync(query, token);

            IEnumerable<TItem> itemsList = azureResult.Results.Select(x => AzureTableClient.Builder.Build<TItem>(x as DynamicTableEntity));
            string nextPageToken = azureResult.ContinuationToken.Serialize();
            return new SegmentedResult<TItem>(itemsList, nextPageToken, null != nextPageToken);
        }
    }
}