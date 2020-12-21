using System;
using System.Threading;
using System.Threading.Tasks;
using Adriva.Storage.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Adriva.Storage.SqlServer
{
#warning Must implement dbcontext methods to persist / retrieve messages
    internal class SqlServerQueueClient : IQueueClient
    {
        private static readonly TimeSpan MinimumVisibilityTimeout = TimeSpan.FromSeconds(30);
        private static readonly TimeSpan MaximumVisibilityTimeout = TimeSpan.FromMinutes(30);

        private readonly QueueDbContext DbContext;
        private readonly IQueueMessageSerializer MessageSerializer;
        private StorageClientContext Context;

        public SqlServerQueueClient(QueueDbContext queueDbContext, IQueueMessageSerializer messageSerializer)
        {
            this.DbContext = queueDbContext;
            this.MessageSerializer = messageSerializer;
        }

        private static void EnsureValidQueueMessage(QueueMessage queueMessage, ref long? id)
        {
            if (null == queueMessage) throw new ArgumentNullException(nameof(queueMessage));

            if (null != id)
            {
                if (!long.TryParse(queueMessage.Id, out long messageId))
                {
                    throw new InvalidOperationException("Message id is invalid and cannot be used with SqlServerQueueClient.");
                }

                id = messageId;
            }
        }

        public ValueTask InitializeAsync(StorageClientContext context)
        {
            var options = context.GetOptions<SqlServerQueueOptions>();
            this.Context = context;
            return new ValueTask();
        }

        public async ValueTask AddAsync(QueueMessage message, TimeSpan? visibilityTimeout = null, TimeSpan? initialVisibilityDelay = null)
        {
            long? messageId = null;
            SqlServerQueueClient.EnsureValidQueueMessage(message, ref messageId);

            visibilityTimeout = visibilityTimeout ?? SqlServerQueueClient.MinimumVisibilityTimeout;

            if (visibilityTimeout < SqlServerQueueClient.MinimumVisibilityTimeout)
            {
                visibilityTimeout = SqlServerQueueClient.MinimumVisibilityTimeout;
            }
            else if (visibilityTimeout > SqlServerQueueClient.MaximumVisibilityTimeout)
            {
                visibilityTimeout = SqlServerQueueClient.MaximumVisibilityTimeout;
            }

            initialVisibilityDelay = initialVisibilityDelay ?? TimeSpan.Zero;

            QueueMessageEntity entity = new QueueMessageEntity();
            entity.Environment = this.Context.Name;
            entity.TimestampUtc = DateTime.UtcNow.AddSeconds(initialVisibilityDelay.Value.TotalSeconds);
            entity.VisibilityTimeout = (int)visibilityTimeout.Value.TotalSeconds;
            entity.RetrievedOnUtc = null;
            entity.Content = this.MessageSerializer.Serialize(message);
            entity.Flags = message.Flags;

            await this.DbContext.AddAsync(entity);
            await this.DbContext.SaveChangesAsync();
            this.DbContext.Entry(entity).State = EntityState.Detached;

            message.SetId(Convert.ToString(entity.Id));
        }

        public async Task DeleteAsync(QueueMessage message)
        {
            long? messageId = null;
            SqlServerQueueClient.EnsureValidQueueMessage(message, ref messageId);
            await this.DbContext.Database.ExecuteSqlRawAsync("?? {0}", messageId);
        }

        public async Task<QueueMessage> GetNextAsync(CancellationToken cancellationToken)
        {
            var messageEntity = await this.DbContext.Messages.FromSqlRaw("?? {0}", this.Context.Name).FirstOrDefaultAsync();
            if (null == messageEntity) return null;

            QueueMessage queueMessage = QueueMessage.Create(null, null, null);
            queueMessage.SetId(Convert.ToString(messageEntity.Id));
            return queueMessage;
        }

        public ValueTask DisposeAsync() => new ValueTask();
    }
}
