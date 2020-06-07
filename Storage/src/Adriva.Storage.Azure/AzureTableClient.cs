using System.IO;
using System.Threading.Tasks;
using Adriva.Storage.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Azure.Cosmos.Table;

namespace Adriva.Storage.Azure
{
    public sealed class AzureTableClient : ITableClient
    {
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
    }
}
