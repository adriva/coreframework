using System;
using Adriva.Storage.Abstractions;
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
                var options = optionsMonitor.Get(Helpers.GetQueueName(name));
                dbContextBuilder.UseSqlServer(options.ConnectionString);
            }, serviceLifetime, serviceLifetime);
            return builder;
        }
    }
}