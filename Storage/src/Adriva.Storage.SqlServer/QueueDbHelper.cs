using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace Adriva.Storage.SqlServer
{
    internal sealed class QueueDbHelper
    {
        private static class ColumnNames
        {
            public const string Id = "Id";
            public const string Application = "Application";
            public const string Environment = "Environment";
            public const string Content = "Content";
            public const string Command = "Command";
            public const string Flags = "Flags";
            public const string VisibilityTimeout = "VisibilityTimeout";
            public const string TimeToLive = "TimeToLive";
            public const string TimestampUtc = "TimestampUtc";
            public const string RetrievedOnUtc = "RetrievedOnUtc";
        }

        private const string AddMessageFormat =
                   "INSERT INTO {0} (Application, Environment, Content, Command, Flags, VisibilityTimeout, TimeToLive, TimestampUtc, RetrievedOnUtc)" +
                   "VALUES(@Application, @Environment, @Content, @Command, @Flags, @VisibilityTimeout, @TimeToLive, @TimestampUtc, @RetrievedOnUtc);" +
                   "SELECT SCOPE_IDENTITY()";

        private const string DeleteMessageFormat = "DELETE {0} WHERE Id = @Id";

        private const string RetrieveMessageFormat = "EXEC {0} @Environment, @Application";

        private string AddMessageQuery { get; }

        private string DeleteMessageQuery { get; }

        private string RetrieveQuery { get; }

        private readonly Dictionary<string, int> ColumnIndices = new Dictionary<string, int>();

        private readonly SqlServerQueueOptions Options;

        public QueueDbHelper(SqlServerQueueOptions queueOptions)
        {
            this.Options = queueOptions;

            this.AddMessageQuery = string.Format(CultureInfo.InvariantCulture, QueueDbHelper.AddMessageFormat, DbHelpers.GetQualifiedTableName(queueOptions));
            this.DeleteMessageQuery = string.Format(CultureInfo.InvariantCulture, QueueDbHelper.DeleteMessageFormat, DbHelpers.GetQualifiedTableName(queueOptions));
            this.RetrieveQuery = string.Format(CultureInfo.InvariantCulture, QueueDbHelper.RetrieveMessageFormat, DbHelpers.GetQualifiedProcedureName(this.Options));
        }

        public async Task<long> AddMessageAsync(QueueMessageEntity queueMessageEntity)
        {
            using (var connection = new SqlConnection(this.Options.ConnectionString))
            using (var command = new SqlCommand(this.AddMessageQuery, connection))
            {
                command.Parameters.AddRange(new[] {
                    new SqlParameter(ColumnNames.Application, queueMessageEntity.Application),
                    new SqlParameter(ColumnNames.Command, queueMessageEntity.Command),
                    new SqlParameter(ColumnNames.Content, queueMessageEntity.Content),
                    new SqlParameter(ColumnNames.Environment, queueMessageEntity.Environment),
                    new SqlParameter(ColumnNames.Flags, queueMessageEntity.Flags),
                    new SqlParameter(ColumnNames.RetrievedOnUtc, (object)queueMessageEntity.RetrievedOnUtc ?? DBNull.Value),
                    new SqlParameter(ColumnNames.TimestampUtc, queueMessageEntity.TimestampUtc),
                    new SqlParameter(ColumnNames.TimeToLive, queueMessageEntity.TimeToLive),
                    new SqlParameter(ColumnNames.VisibilityTimeout, queueMessageEntity.VisibilityTimeout)
                });

                await connection.OpenAsync();

                var result = await command.ExecuteScalarAsync();

                return Convert.ToInt64(result);
            }
        }

        public async Task DeleteAsync(long? messageId)
        {
            if (!messageId.HasValue)
            {
                return;
            }

            using (var connection = new SqlConnection(this.Options.ConnectionString))
            using (var command = new SqlCommand(this.DeleteMessageQuery, connection))
            {
                command.Parameters.Add(new SqlParameter(ColumnNames.Id, messageId));

                await connection.OpenAsync();

                await command.ExecuteNonQueryAsync();
            }
        }

        public async Task<QueueMessageEntity> GetNextAsync(string environmentName, CancellationToken cancellationToken)
        {
            using (var connection = new SqlConnection(this.Options.ConnectionString))
            using (var command = new SqlCommand(this.RetrieveQuery, connection))
            {
                command.CommandType = CommandType.Text;
                command.Parameters.AddRange(new[]{
                    new SqlParameter(ColumnNames.Application, this.Options.ApplicationName),
                    new SqlParameter(ColumnNames.Environment, environmentName),
                });

                await connection.OpenAsync();

                using (var reader = await command.ExecuteReaderAsync(System.Data.CommandBehavior.SingleResult | System.Data.CommandBehavior.SingleRow, cancellationToken))
                {
                    if (await reader.ReadAsync(cancellationToken))
                    {
                        if (0 == this.ColumnIndices.Count)
                        {
                            lock (this.ColumnIndices)
                            {
                                if (0 == this.ColumnIndices.Count)
                                {
                                    this.ColumnIndices[ColumnNames.Application] = reader.GetOrdinal(ColumnNames.Application);
                                    this.ColumnIndices[ColumnNames.Command] = reader.GetOrdinal(ColumnNames.Command);
                                    this.ColumnIndices[ColumnNames.Content] = reader.GetOrdinal(ColumnNames.Content);
                                    this.ColumnIndices[ColumnNames.Environment] = reader.GetOrdinal(ColumnNames.Environment);
                                    this.ColumnIndices[ColumnNames.Flags] = reader.GetOrdinal(ColumnNames.Flags);
                                    this.ColumnIndices[ColumnNames.Id] = reader.GetOrdinal(ColumnNames.Id);
                                    this.ColumnIndices[ColumnNames.RetrievedOnUtc] = reader.GetOrdinal(ColumnNames.RetrievedOnUtc);
                                    this.ColumnIndices[ColumnNames.TimestampUtc] = reader.GetOrdinal(ColumnNames.TimestampUtc);
                                    this.ColumnIndices[ColumnNames.TimeToLive] = reader.GetOrdinal(ColumnNames.TimeToLive);
                                    this.ColumnIndices[ColumnNames.VisibilityTimeout] = reader.GetOrdinal(ColumnNames.VisibilityTimeout);
                                }
                            }
                        }

                        QueueMessageEntity entity = new QueueMessageEntity();
                        entity.Application = await reader.GetFieldValueAsync<string>(this.ColumnIndices[ColumnNames.Application]);
                        entity.Command = await reader.GetFieldValueAsync<string>(this.ColumnIndices[ColumnNames.Command]);
                        entity.Content = await reader.GetFieldValueAsync<string>(this.ColumnIndices[ColumnNames.Content]);
                        entity.Environment = await reader.GetFieldValueAsync<string>(this.ColumnIndices[ColumnNames.Environment]);
                        entity.Flags = await reader.GetFieldValueAsync<long>(this.ColumnIndices[ColumnNames.Flags]);
                        entity.Id = await reader.GetFieldValueAsync<long>(this.ColumnIndices[ColumnNames.Id]);
                        entity.RetrievedOnUtc = await reader.GetFieldValueAsync<DateTime?>(this.ColumnIndices[ColumnNames.RetrievedOnUtc]);
                        entity.TimestampUtc = await reader.GetFieldValueAsync<DateTime>(this.ColumnIndices[ColumnNames.TimestampUtc]);
                        entity.TimeToLive = await reader.GetFieldValueAsync<int>(this.ColumnIndices[ColumnNames.TimeToLive]);
                        entity.VisibilityTimeout = await reader.GetFieldValueAsync<int>(this.ColumnIndices[ColumnNames.VisibilityTimeout]);
                        return entity;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
        }
    }
}
