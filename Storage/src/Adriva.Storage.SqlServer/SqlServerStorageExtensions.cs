using System;
using Adriva.Storage.SqlServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class SqlServerStorageExtensions
    {
        public static IStorageBuilder AddSqlServerQueue(this IStorageBuilder builder, string name, Action<SqlServerQueueOptions> configure)
        {
            builder.AddQueueClient<SqlServerQueueClient, SqlServerQueueOptions>(name, configure);
            builder.Services.TryAddSingleton<DbContextFactory>();
            return builder;
        }

        public static IStorageBuilder AddSqlServerBlob(this IStorageBuilder builder, string name, Action<SqlServerBlobOptions> configure)
        {
            builder.AddBlobClient<SqlServerBlobClient, SqlServerBlobOptions>(name, configure);
            builder.Services.TryAddSingleton<DbContextFactory>();
            return builder;
        }
    }
}