using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Adriva.Storage.Abstractions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Adriva.Storage.SqlServer
{
    internal class SqlServerQueueClient : IQueueClient
    {
        private static readonly TimeSpan MinimumVisibilityTimeout = TimeSpan.FromSeconds(30);
        private static readonly TimeSpan MaximumVisibilityTimeout = TimeSpan.FromMinutes(30);
        private static readonly TimeSpan DefaultTimeToLive = TimeSpan.FromHours(24);
        private static readonly SemaphoreSlim DatabaseCreateSemaphore = new SemaphoreSlim(1, 1);

        private static bool IsDatabaseObjectsCreated = false;

        private readonly QueueDbContext DbContext;
        private readonly IQueueMessageSerializer MessageSerializer;

        private SqlServerQueueOptions Options;
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
                        await DbHelpers.ExecuteScriptAsync(this.DbContext.Database, this.Options, "queue-createtable", "queue-createsp");
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
            this.Options = context.GetOptions<SqlServerQueueOptions>();
            await this.EnsureDatabaseObjectsAsync();
            this.Context = context;
        }

        public async ValueTask AddAsync(QueueMessage message, TimeSpan? timeToLive = null, TimeSpan? visibilityTimeout = null, TimeSpan? initialVisibilityDelay = null)
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

            timeToLive = timeToLive ?? SqlServerQueueClient.DefaultTimeToLive;

            initialVisibilityDelay = initialVisibilityDelay ?? TimeSpan.Zero;

            QueueMessageEntity entity = new QueueMessageEntity();
            entity.Environment = this.Context.Name;
            entity.Application = this.Options.ApplicationName;
            entity.TimestampUtc = DateTime.UtcNow.AddSeconds(initialVisibilityDelay.Value.TotalSeconds);
            entity.VisibilityTimeout = (int)visibilityTimeout.Value.TotalSeconds;
            entity.RetrievedOnUtc = null;
            entity.Content = this.MessageSerializer.Serialize(message);
            entity.Flags = message.Flags;
            entity.Command = message.CommandType;
            entity.TimeToLive = (int)timeToLive.Value.TotalSeconds;

            await this.DbContext.AddAsync(entity);
            await this.DbContext.SaveChangesAsync();
            this.DbContext.Entry(entity).State = EntityState.Detached;

            message.SetId(Convert.ToString(entity.Id));
        }

        public async Task DeleteAsync(QueueMessage message)
        {
            long? messageId = -1;
            SqlServerQueueClient.EnsureValidQueueMessage(message, ref messageId);
            await this.DbContext.Database.ExecuteSqlRawAsync($"DELETE {this.Options.SchemaName}.{this.Options.TableName} WHERE Id = @messageId", new SqlParameter("@messageId", messageId));
        }

        public async Task<QueueMessage> GetNextAsync(CancellationToken cancellationToken)
        {
            var resultset = await this.DbContext.Messages.FromSqlRaw($"EXEC {this.Options.SchemaName}.{this.Options.RetrieveProcedureName} @environment, @application", new SqlParameter("@environment", this.Context.Name), new SqlParameter("@application", this.Options.ApplicationName)).ToArrayAsync();
            var messageEntity = resultset.FirstOrDefault();
            if (null == messageEntity) return null;

            var queueMessage = this.MessageSerializer.Deserialize(messageEntity.Content);
            queueMessage.SetId(Convert.ToString(messageEntity.Id));
            return queueMessage;
        }
    }
}
