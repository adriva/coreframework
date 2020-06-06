using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Adriva.Storage.Abstractions;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Queue;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Adriva.Storage.Azure
{
    public sealed class AzureQueueClient : IQueueClient
    {
        private readonly IServiceProvider ServiceProvider;
        private readonly IOptionsMonitor<AzureQueueConfiguration> ConfigurationAccessor;

        private AzureQueueConfiguration Configuration;
        private CloudQueue Queue;
        private IQueueMessageSerializer Serializer;

        public AzureQueueClient(IServiceProvider serviceProvider, IOptionsMonitor<AzureQueueConfiguration> configuration)
        {
            this.ServiceProvider = serviceProvider;
            this.ConfigurationAccessor = configuration;
        }

        public async ValueTask InitializeAsync(string clientName)
        {
            this.Configuration = this.ConfigurationAccessor.Get(clientName);
            this.Serializer = (IQueueMessageSerializer)ActivatorUtilities.CreateInstance(this.ServiceProvider, this.Configuration.MessageSerializerType);

            if (!CloudStorageAccount.TryParse(this.Configuration.ConnectionString, out CloudStorageAccount account))
            {
                throw new InvalidDataException($"Azure queue connection string for queue client '{clientName}' could not be parsed.");
            }

            var client = account.CreateCloudQueueClient();
            this.Queue = client.GetQueueReference(this.Configuration.QueueName);
            await this.Queue.CreateIfNotExistsAsync();
        }

        public async ValueTask AddAsync(QueueMessage message, TimeSpan? timeToLive = null, TimeSpan? visibilityDelay = null)
        {
            if (null == message) throw new ArgumentNullException(nameof(message));

            timeToLive = timeToLive ?? this.Configuration.DefaultTimeToLive;

            byte[] content = await this.Serializer.SerializeAsync(message);
            CloudQueueMessage cloudQueueMessage = new CloudQueueMessage(content);
            await this.Queue.AddMessageAsync(cloudQueueMessage, timeToLive, visibilityDelay, null, null);
        }

        public async Task<QueueMessage> GetNextAsync(CancellationToken cancellationToken)
        {
            CloudQueueMessage cloudQueueMessage = await this.Queue.GetMessageAsync(cancellationToken);
            if (null == cloudQueueMessage) return null;

            return await this.Serializer.DeserializeAsync(cloudQueueMessage.AsBytes);
        }

        public ValueTask DisposeAsync()
        {
            this.Queue = null;
            return new ValueTask();
        }
    }
}
