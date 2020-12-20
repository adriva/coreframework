using System;
using System.Threading;
using System.Threading.Tasks;
using Adriva.Storage.Abstractions;
using Microsoft.Extensions.Options;

namespace Adriva.Storage.SqlServer
{

    internal class SqlServerQueueClient : IQueueClient
    {
        public SqlServerQueueClient(QueueDbContext queueDbContext)
        {
        }

        public ValueTask InitializeAsync(StorageClientContext context)
        {
            var options = context.GetOptions<SqlServerQueueOptions>();
            return new ValueTask();
        }

        public ValueTask AddAsync(QueueMessage message, TimeSpan? timeToLive = null, TimeSpan? initialVisibilityDelay = null)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(QueueMessage message)
        {
            throw new NotImplementedException();
        }

        public Task<QueueMessage> GetNextAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public ValueTask DisposeAsync()
        {
            throw new NotImplementedException();
        }
    }
}
