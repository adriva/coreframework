using System;
using System.Threading;
using System.Threading.Tasks;
using Adriva.Common.Core;
using Adriva.Storage.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

namespace Adriva.Storage.SqlServer
{
    internal class SqlServerQueueClient : IQueueClient
    {
        private static readonly TimeSpan MinimumVisibilityTimeout = TimeSpan.FromSeconds(30);
        private static readonly TimeSpan MaximumVisibilityTimeout = TimeSpan.FromMinutes(30);
        private static readonly SemaphoreSlim DatabaseCreateSemaphore = new SemaphoreSlim(1, 1);

        private static bool IsDatabaseObjectsCreated = false;

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

        private async ValueTask EnsureDatabaseObjectsAsync()
        {
            if (!SqlServerQueueClient.IsDatabaseObjectsCreated)
            {
                await SqlServerQueueClient.DatabaseCreateSemaphore.WaitAsync();

                try
                {
                    if (!SqlServerQueueClient.IsDatabaseObjectsCreated)
                    {
                        var resourceFileProvider = new EmbeddedFileProvider(typeof(SqlServerQueueClient).Assembly);
                        var storedProcedureFileInfo = resourceFileProvider.GetFileInfo("createsp.sql");
                        var tableFileInfo = resourceFileProvider.GetFileInfo("createtable.sql");

                        string sqlStoredProcedure = await storedProcedureFileInfo.ReadAllTextAsync();
                        string sqlTable = await tableFileInfo.ReadAllTextAsync();

                        await this.DbContext.Database.ExecuteSqlRawAsync(sqlTable);
                        await this.DbContext.Database.ExecuteSqlRawAsync(sqlStoredProcedure);
                        SqlServerQueueClient.IsDatabaseObjectsCreated = true;
                    }
                }
                finally
                {
                    SqlServerQueueClient.DatabaseCreateSemaphore.Release();
                }
            }
        }

        public async ValueTask InitializeAsync(StorageClientContext context)
        {
            await this.EnsureDatabaseObjectsAsync();
            var options = context.GetOptions<SqlServerQueueOptions>();
            this.Context = context;
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
            long? messageId = -1;
            SqlServerQueueClient.EnsureValidQueueMessage(message, ref messageId);
            await this.DbContext.Database.ExecuteSqlRawAsync("DELETE [QueueMessages] WHERE Id = {0}", messageId);
        }

        public async Task<QueueMessage> GetNextAsync(CancellationToken cancellationToken)
        {
            var messageEntity = await this.DbContext.Messages.FromSqlRaw("GetNextQueueMessage {0}", this.Context.Name).FirstOrDefaultAsync();
            if (null == messageEntity) return null;

            QueueMessage queueMessage = QueueMessage.Create(null, null, null);
            queueMessage.SetId(Convert.ToString(messageEntity.Id));
            return queueMessage;
        }

        public ValueTask DisposeAsync()
        {
            return new ValueTask();
        }
    }
}
