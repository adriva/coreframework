using System;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Caching.Distributed;

namespace Adriva.Extensions.Caching.SqlServer
{
    internal class DatabaseOperations(string connectionString, string schemaName, string tableName, string dependencyTableName) : IDatabaseOperations
    {
        /// <summary>
        /// Since there is no specific exception type representing a 'duplicate key' error, we are relying on
        /// the following message number which represents the following text in Microsoft SQL Server database.
        ///     "Violation of %ls constraint '%.*ls'. Cannot insert duplicate key in object '%.*ls'.
        ///     The duplicate key value is %ls."
        /// You can find the list of system messages by executing the following query:
        /// "SELECT * FROM sys.messages WHERE [text] LIKE '%duplicate%'"
        /// </summary>
        private const int DuplicateKeyErrorId = 2627;

        protected const string GetTableSchemaErrorText =
            "Could not retrieve information of table with schema '{0}' and " +
            "name '{1}'. Make sure you have the table setup and try again. " +
            "Connection string: {2}";

        protected SqlQueries SqlQueries { get; } = new SqlQueries(schemaName, tableName, dependencyTableName);

        protected string ConnectionString { get; } = connectionString;

        protected string SchemaName { get; } = schemaName;

        protected string TableName { get; } = tableName;

        public void DeleteCacheItem(string key)
        {
            using var connection = new SqlConnection(ConnectionString);
            using var command = new SqlCommand(SqlQueries.DeleteCacheItem, connection);
            command.Parameters.AddCacheItemId(key);

            connection.Open();

            command.ExecuteNonQuery();
        }

        public async Task DeleteCacheItemAsync(string key, CancellationToken token = default(CancellationToken))
        {
            token.ThrowIfCancellationRequested();

            using (var connection = new SqlConnection(ConnectionString))
            using (var command = new SqlCommand(SqlQueries.DeleteCacheItem, connection))
            {
                command.Parameters.AddCacheItemId(key);

                await connection.OpenAsync(token).ConfigureAwait(false);

                await command.ExecuteNonQueryAsync(token).ConfigureAwait(false);
            }
        }

        public virtual byte[] GetCacheItem(string key)
        {
            return GetCacheItem(key, includeValue: true);
        }

        public virtual async Task<byte[]> GetCacheItemAsync(string key, CancellationToken token = default(CancellationToken))
        {
            token.ThrowIfCancellationRequested();

            return await this.GetCacheItemAsync(key, includeValue: true, token: token).ConfigureAwait(false);
        }

        public void RefreshCacheItem(string key)
        {
            GetCacheItem(key, includeValue: false);
        }

        public async Task RefreshCacheItemAsync(string key, CancellationToken token = default(CancellationToken))
        {
            token.ThrowIfCancellationRequested();

            await GetCacheItemAsync(key, includeValue: false, token: token).ConfigureAwait(false);
        }

        public virtual void DeleteExpiredCacheItems()
        {
            var utcNow = DateTimeOffset.UtcNow;

            using var connection = new SqlConnection(ConnectionString);
            using var deleteExpiredCacheItemsCommand = new SqlCommand(SqlQueries.DeleteExpiredCacheItems, connection);
            using var deleteExpiredDependencyMonikersCommand = new SqlCommand(SqlQueries.DeleteExpiredDependencyMonikers, connection);

            deleteExpiredCacheItemsCommand.Parameters.AddWithValue("UtcNow", SqlDbType.DateTime2, utcNow);

            connection.Open();

            _ = deleteExpiredCacheItemsCommand.ExecuteNonQuery();
            _ = deleteExpiredDependencyMonikersCommand.ExecuteNonQuery();
        }

        public async Task DeleteAllItemsAsync()
        {
            using var connection = new SqlConnection(ConnectionString);
            using var transaction = await connection.BeginTransactionAsync(IsolationLevel.Serializable);
            using var deleteAllItemsCommand = new SqlCommand(SqlQueries.DeleteAllItems, connection);

            connection.Open();
            try
            {
                await deleteAllItemsCommand.ExecuteNonQueryAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
            }
        }

        public virtual void SetCacheItem(string key, byte[] value, DistributedCacheEntryOptions options)
        {
            var utcNow = DateTimeOffset.UtcNow;

            var absoluteExpiration = DatabaseOperations.GetAbsoluteExpiration(utcNow, options);
            DatabaseOperations.ValidateOptions(options.SlidingExpiration, absoluteExpiration);

            using var connection = new SqlConnection(ConnectionString);
            using var upsertCommand = new SqlCommand(SqlQueries.SetCacheItem, connection);

            upsertCommand.Parameters
                .AddCacheItemId(key)
                .AddCacheItemValue(value)
                .AddSlidingExpirationInSeconds(options.SlidingExpiration)
                .AddAbsoluteExpiration(absoluteExpiration)
                .AddWithValue("UtcNow", SqlDbType.DateTime2, utcNow);

            connection.Open();

            try
            {
                upsertCommand.ExecuteNonQuery();
            }
            catch (SqlException ex)
            {
                if (DatabaseOperations.IsDuplicateKeyException(ex))
                {
                    // There is a possibility that multiple requests can try to add the same item to the cache, in
                    // which case we receive a 'duplicate key' exception on the primary key column.
                }
                else
                {
                    throw;
                }
            }
        }

        public virtual async Task SetCacheItemAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default(CancellationToken))
        {
            token.ThrowIfCancellationRequested();

            var utcNow = DateTimeOffset.UtcNow;

            var absoluteExpiration = DatabaseOperations.GetAbsoluteExpiration(utcNow, options);
            DatabaseOperations.ValidateOptions(options.SlidingExpiration, absoluteExpiration);

            using (var connection = new SqlConnection(ConnectionString))
            using (var upsertCommand = new SqlCommand(SqlQueries.SetCacheItem, connection))
            {
                upsertCommand.Parameters
                    .AddCacheItemId(key)
                    .AddCacheItemValue(value)
                    .AddSlidingExpirationInSeconds(options.SlidingExpiration)
                    .AddAbsoluteExpiration(absoluteExpiration)
                    .AddWithValue("UtcNow", SqlDbType.DateTime2, utcNow);

                await connection.OpenAsync(token).ConfigureAwait(false);

                try
                {
                    await upsertCommand.ExecuteNonQueryAsync(token).ConfigureAwait(false);
                }
                catch (SqlException ex)
                {
                    if (DatabaseOperations.IsDuplicateKeyException(ex))
                    {
                        // There is a possibility that multiple requests can try to add the same item to the cache, in
                        // which case we receive a 'duplicate key' exception on the primary key column.
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }

        protected virtual byte[] GetCacheItem(string key, bool includeValue)
        {
            var utcNow = DateTimeOffset.UtcNow;

            string query;
            if (includeValue)
            {
                query = SqlQueries.GetCacheItem;
            }
            else
            {
                query = SqlQueries.GetCacheItemWithoutValue;
            }

            byte[] value = null;
            using (var connection = new SqlConnection(ConnectionString))
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters
                    .AddCacheItemId(key)
                    .AddWithValue("UtcNow", SqlDbType.DateTime2, utcNow);

                connection.Open();

                using (var reader = command.ExecuteReader(
                    CommandBehavior.SequentialAccess | CommandBehavior.SingleRow | CommandBehavior.SingleResult))
                {
                    if (reader.Read())
                    {
                        if (includeValue)
                        {
                            value = reader.GetFieldValue<byte[]>(Columns.Indexes.CacheItemValueIndex);
                        }
                    }
                    else
                    {
                        return null;
                    }
                }
            }

            return value;
        }

        protected virtual async Task<byte[]> GetCacheItemAsync(string key, bool includeValue, CancellationToken token = default(CancellationToken))
        {
            token.ThrowIfCancellationRequested();

            var utcNow = DateTimeOffset.UtcNow;

            string query;
            if (includeValue)
            {
                query = SqlQueries.GetCacheItem;
            }
            else
            {
                query = SqlQueries.GetCacheItemWithoutValue;
            }

            byte[] value = null;

            using (var connection = new SqlConnection(ConnectionString))
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters
                    .AddCacheItemId(key)
                    .AddWithValue("UtcNow", SqlDbType.DateTime2, utcNow);

                await connection.OpenAsync(token).ConfigureAwait(false);

                using (var reader = await command.ExecuteReaderAsync(CommandBehavior.SequentialAccess | CommandBehavior.SingleRow | CommandBehavior.SingleResult, token).ConfigureAwait(false))
                {
                    if (await reader.ReadAsync(token).ConfigureAwait(false))
                    {
                        if (includeValue)
                        {
                            value = await reader.GetFieldValueAsync<byte[]>(Columns.Indexes.CacheItemValueIndex, token).ConfigureAwait(false);
                        }
                    }
                    else
                    {
                        return null;
                    }
                }
            }

            return value;
        }

        public async Task AddOrUpdateDependencyMonikersAsync(string key, string[] dependencyMonikers)
        {
            if (null == dependencyMonikers || 0 == dependencyMonikers.Length)
            {
                return;
            }

            using (var connection = new SqlConnection(ConnectionString))
            using (var command = new SqlCommand(SqlQueries.AddOrUpdateDependencyMoniker, connection))
            {
                command.Parameters.AddWithValue(Columns.DependencyNames.CacheKey, key);

                await connection.OpenAsync();

                foreach (string dependencyMoniker in dependencyMonikers)
                {
                    if (!string.IsNullOrWhiteSpace(dependencyMoniker))
                    {
                        SqlParameter monikerParameter = new SqlParameter(Columns.DependencyNames.Key, dependencyMoniker);

                        command.Parameters.Add(monikerParameter);
                        await command.ExecuteNonQueryAsync();
                        command.Parameters.Remove(monikerParameter);
                    }
                }
            }
        }

        public void NotifyChanged(string key, string dependencyMoniker)
        {
            if (string.IsNullOrWhiteSpace(dependencyMoniker))
            {
                return;
            }

            using (var connection = new SqlConnection(ConnectionString))
            using (var command = new SqlCommand(SqlQueries.NotifyChanged, connection))
            {
                command.Parameters.AddWithValue(Columns.DependencyNames.Key, dependencyMoniker);
                command.Parameters.AddWithValue(Columns.Names.CacheItemId, key ?? string.Empty);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        public async Task NotifyChangedAsync(string key, string dependencyMoniker)
        {
            if (string.IsNullOrWhiteSpace(dependencyMoniker))
            {
                return;
            }

            using (var connection = new SqlConnection(ConnectionString))
            using (var command = new SqlCommand(SqlQueries.NotifyChanged, connection))
            {
                command.Parameters.AddWithValue(Columns.DependencyNames.Key, dependencyMoniker);
                command.Parameters.AddWithValue(Columns.Names.CacheItemId, key ?? string.Empty);

                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();
            }
        }

        protected static bool IsDuplicateKeyException(SqlException ex)
        {
            if (ex.Errors != null)
            {
                return ex.Errors.Cast<SqlError>().Any(error => error.Number == DuplicateKeyErrorId);
            }
            return false;
        }

        protected static DateTimeOffset? GetAbsoluteExpiration(DateTimeOffset utcNow, DistributedCacheEntryOptions options)
        {
            // calculate absolute expiration
            DateTimeOffset? absoluteExpiration = null;
            if (options.AbsoluteExpirationRelativeToNow.HasValue)
            {
                absoluteExpiration = utcNow.Add(options.AbsoluteExpirationRelativeToNow.Value);
            }
            else if (options.AbsoluteExpiration.HasValue)
            {
                if (options.AbsoluteExpiration.Value <= utcNow)
                {
                    throw new InvalidOperationException("The absolute expiration value must be in the future.");
                }

                absoluteExpiration = options.AbsoluteExpiration.Value;
            }
            return absoluteExpiration;
        }

        protected static void ValidateOptions(TimeSpan? slidingExpiration, DateTimeOffset? absoluteExpiration)
        {
            if (!slidingExpiration.HasValue && !absoluteExpiration.HasValue)
            {
                throw new InvalidOperationException("Either absolute or sliding expiration needs " +
                    "to be provided.");
            }
        }
    }
}