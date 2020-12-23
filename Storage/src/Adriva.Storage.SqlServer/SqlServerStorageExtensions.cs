using System;
using Adriva.Storage.SqlServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class SqlServerStorageExtensions
    {
        public static IStorageBuilder AddSqlServerQueue(this IStorageBuilder builder, string name, Action<SqlServerQueueOptions> configure)
        {
            builder.AddQueueClient<SqlServerQueueClient, SqlServerQueueOptions>(name, configure);
            builder.Services.AddDbContext<QueueDbContext>((serviceProvider, dbContextBuilder) =>
            {
                var optionsMonitor = serviceProvider.GetRequiredService<IOptionsMonitor<SqlServerQueueOptions>>();
                var options = optionsMonitor.Get(Helpers.GetQualifiedQueueName(name));
                dbContextBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
                dbContextBuilder.UseSqlServer(options.ConnectionString);
            }, ServiceLifetime.Singleton, ServiceLifetime.Singleton);
            return builder;
        }

        public static IStorageBuilder AddSqlServerBlob(this IStorageBuilder builder, string name, Action<SqlServerBlobOptions> configure)
        {
            builder.AddBlobClient<SqlServerBlobClient, SqlServerBlobOptions>(name, configure);
            builder.Services.AddDbContext<BlobDbContext>((serviceProvider, dbContextBuilder) =>
            {
                var optionsMonitor = serviceProvider.GetRequiredService<IOptionsMonitor<SqlServerBlobOptions>>();
                var options = optionsMonitor.Get(Helpers.GetQualifiedBlobName(name));
                dbContextBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
                dbContextBuilder.UseSqlServer(options.ConnectionString);
            }, ServiceLifetime.Singleton, ServiceLifetime.Singleton);
            return builder;
        }
    }
}