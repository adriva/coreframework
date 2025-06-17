using System;
using Adriva.Storage.SqlServer;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class SqlServerStorageExtensions
    {
        public static IStorageBuilder AddSqlServerQueue(this IStorageBuilder builder, string name, Action<SqlServerQueueOptions> configure)
        {
            builder.AddQueueClient<SqlServerQueueClient, SqlServerQueueOptions>(name, configure);
            return builder;
        }
    }
}