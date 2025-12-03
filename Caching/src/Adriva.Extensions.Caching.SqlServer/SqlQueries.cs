// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Globalization;

namespace Adriva.Extensions.Caching.SqlServer
{
    internal class SqlQueries
    {
        private const string TableInfoFormat =
            "SELECT TABLE_CATALOG, TABLE_SCHEMA, TABLE_NAME, TABLE_TYPE " +
            "FROM INFORMATION_SCHEMA.TABLES " +
            "WHERE TABLE_SCHEMA = '{0}' " +
            "AND TABLE_NAME = '{1}'";

        private const string UpdateCacheItemFormat =
        "UPDATE {0} " +
        "SET ExpiresAtTime = " +
            "(CASE " +
                "WHEN DATEDIFF(SECOND, @UtcNow, AbsoluteExpiration) <= SlidingExpirationInSeconds " +
                "THEN AbsoluteExpiration " +
                "ELSE " +
                "DATEADD(SECOND, SlidingExpirationInSeconds, @UtcNow) " +
            "END) " +
        "WHERE Id = @Id " +
        "AND @UtcNow <= ExpiresAtTime " +
        "AND SlidingExpirationInSeconds IS NOT NULL " +
        "AND (AbsoluteExpiration IS NULL OR AbsoluteExpiration <> ExpiresAtTime) ;";

        private const string GetCacheItemFormat =
            "SELECT Value " +
            "FROM {0} WHERE Id = @Id AND @UtcNow <= ExpiresAtTime;";

        private const string SetCacheItemFormat =
            "DECLARE @ExpiresAtTime DATETIME2(7); " +
            "SET @ExpiresAtTime = " +
            "(CASE " +
                    "WHEN (@SlidingExpirationInSeconds IS NUll) " +
                    "THEN @AbsoluteExpiration " +
                    "ELSE " +
                    "DATEADD(SECOND, Convert(bigint, @SlidingExpirationInSeconds), @UtcNow) " +
            "END);" +
            "UPDATE {0} SET Value = @Value, ExpiresAtTime = @ExpiresAtTime," +
            "SlidingExpirationInSeconds = @SlidingExpirationInSeconds, AbsoluteExpiration = @AbsoluteExpiration " +
            "WHERE Id = @Id " +
            "IF (@@ROWCOUNT = 0) " +
            "BEGIN " +
                "INSERT INTO {0} " +
                "(Id, Value, ExpiresAtTime, SlidingExpirationInSeconds, AbsoluteExpiration) " +
                "VALUES (@Id, @Value, @ExpiresAtTime, @SlidingExpirationInSeconds, @AbsoluteExpiration); " +
            "END ";

        private const string UpsertDependencyMonikerFormat =
            "IF NOT EXISTS (SELECT * FROM {0} WHERE [Key] = @Key AND CacheKey = @CacheKey)" +
            "BEGIN" +
            "    INSERT INTO {0} ([Key], CacheKey)" +
            "    VALUES (@Key, @CacheKey)" +
            "END";

        private const string NotifyChangedFormat =
            "DELETE {0} WHERE Id IN (" +
            "    SELECT [CacheKey] FROM {1} WHERE [Key] = @Key AND [CacheKey] <> @Id" +
            ");" +
            "DELETE {1} WHERE [Key] = @Key AND [CacheKey] <> @Id";

        private const string DeleteCacheItemFormat = "DELETE FROM {0} WHERE Id = @Id";

        public const string DeleteExpiredCacheItemsFormat = "DELETE FROM {0} WHERE @UtcNow > ExpiresAtTime";

        public const string DeleteExpiredDependencyMonikersFormat = "DELETE FROM {1} WHERE NOT EXISTS (SELECT * FROM {0} WHERE {0}.Id = {1}.CacheKey)";

        public const string DeleteAllItemsFormat = "DELETE {0};DELETE {1}";

        public SqlQueries(string schemaName, string tableName, string dependencyTableName)
        {
            var tableNameWithSchema = string.Format(
                CultureInfo.InvariantCulture,
                "{0}.{1}", DelimitIdentifier(schemaName), DelimitIdentifier(tableName));

            var dependencyTableNameWithSchema = string.Format(
                CultureInfo.InvariantCulture,
                "{0}.{1}", DelimitIdentifier(schemaName), DelimitIdentifier(dependencyTableName));

            // when retrieving an item, we do an UPDATE first and then a SELECT
            this.GetCacheItem = string.Format(CultureInfo.InvariantCulture, UpdateCacheItemFormat + GetCacheItemFormat, tableNameWithSchema);
            this.GetCacheItemWithoutValue = string.Format(CultureInfo.InvariantCulture, UpdateCacheItemFormat, tableNameWithSchema);
            this.DeleteCacheItem = string.Format(CultureInfo.InvariantCulture, DeleteCacheItemFormat, tableNameWithSchema);
            this.DeleteExpiredCacheItems = string.Format(CultureInfo.InvariantCulture, DeleteExpiredCacheItemsFormat, tableNameWithSchema);
            this.DeleteAllItems = string.Format(CultureInfo.InvariantCulture, DeleteAllItemsFormat, dependencyTableNameWithSchema, tableNameWithSchema);
            this.SetCacheItem = string.Format(CultureInfo.InvariantCulture, SetCacheItemFormat, tableNameWithSchema);
            this.TableInfo = string.Format(CultureInfo.InvariantCulture, TableInfoFormat, EscapeLiteral(schemaName), EscapeLiteral(tableName));
            this.AddOrUpdateDependencyMoniker = string.Format(CultureInfo.InvariantCulture, UpsertDependencyMonikerFormat, dependencyTableNameWithSchema);
            this.NotifyChanged = string.Format(CultureInfo.InvariantCulture, NotifyChangedFormat, tableNameWithSchema, dependencyTableNameWithSchema);
            this.DeleteExpiredDependencyMonikers = string.Format(CultureInfo.InvariantCulture, DeleteExpiredDependencyMonikersFormat, tableNameWithSchema, dependencyTableNameWithSchema);
        }

        public string TableInfo { get; }

        public string GetCacheItem { get; }

        public string GetCacheItemWithoutValue { get; }

        public string SetCacheItem { get; }

        public string DeleteCacheItem { get; }

        public string DeleteAllItems { get; }

        public string DeleteExpiredCacheItems { get; }

        public string DeleteExpiredDependencyMonikers { get; }

        public string AddOrUpdateDependencyMoniker { get; }

        public string NotifyChanged { get; }

        // From EF's SqlServerQuerySqlGenerator
        private static string DelimitIdentifier(string identifier)
        {
            return "[" + identifier.Replace("]", "]]") + "]";
        }

        private static string EscapeLiteral(string literal)
        {
            return literal.Replace("'", "''");
        }
    }
}