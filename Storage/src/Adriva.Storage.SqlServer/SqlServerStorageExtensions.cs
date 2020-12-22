using System;
using Adriva.Storage.SqlServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class SqlServerStorageExtensions
    {
        public static IStorageBuilder AddSqlServerQueue(this IStorageBuilder builder, string name, ServiceLifetime serviceLifetime, Action<SqlServerQueueOptions> configure)
        {
            builder.AddQueueClient<SqlServerQueueClient, SqlServerQueueOptions>(name, serviceLifetime, configure);
            builder.Services.AddDbContext<QueueDbContext>((serviceProvider, dbContextBuilder) =>
            {
                var optionsMonitor = serviceProvider.GetRequiredService<IOptionsMonitor<SqlServerQueueOptions>>();
                var options = optionsMonitor.Get(Helpers.GetQualifiedQueueName(name));
                dbContextBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
                dbContextBuilder.UseSqlServer(options.ConnectionString);
            }, serviceLifetime, serviceLifetime);
            return builder;
        }

        public static IStorageBuilder AddSqlServerBlob(this IStorageBuilder builder, string name)
        {
            return builder;
        }
    }
}