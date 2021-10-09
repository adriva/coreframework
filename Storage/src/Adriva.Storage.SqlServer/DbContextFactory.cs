using System;
using System.Collections.Concurrent;
using Adriva.Storage.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Adriva.Storage.SqlServer
{
    internal sealed class DbContextFactory : IDisposable
    {
        private readonly ConcurrentDictionary<string, Lazy<QueueDbContext>> QueueContextInstances = new ConcurrentDictionary<string, Lazy<QueueDbContext>>(StringComparer.OrdinalIgnoreCase);
        private readonly ConcurrentDictionary<string, Lazy<BlobDbContext>> BlobContextInstances = new ConcurrentDictionary<string, Lazy<BlobDbContext>>(StringComparer.OrdinalIgnoreCase);

        internal QueueDbContext GetQueueDbContext(StorageClientContext context, SqlServerQueueOptions options)
        {
            this.QueueContextInstances.GetOrAdd(context.Name, (key) =>
            {
                return new Lazy<QueueDbContext>(() =>
                {
                    DbContextOptionsBuilder<QueueDbContext> builder = new DbContextOptionsBuilder<QueueDbContext>();
                    builder.UseSqlServer(options.ConnectionString, setup => { });
                    return new QueueDbContext(options, builder.Options);
                }, true);
            });

            return this.QueueContextInstances[context.Name].Value;
        }

        internal BlobDbContext GetBlobDbContext(StorageClientContext context, SqlServerBlobOptions options)
        {
            this.BlobContextInstances.GetOrAdd(context.Name, (key) =>
            {
                return new Lazy<BlobDbContext>(() =>
                {
                    DbContextOptionsBuilder<BlobDbContext> builder = new DbContextOptionsBuilder<BlobDbContext>();
                    builder.UseSqlServer(options.ConnectionString, setup => { });
                    return new BlobDbContext(options, builder.Options);
                }, true);
            });

            return this.BlobContextInstances[context.Name].Value;
        }

        public void Dispose()
        {
            foreach (var lazyConstructor in this.QueueContextInstances.Values)
            {
                if (lazyConstructor.IsValueCreated)
                {
                    lazyConstructor.Value.Dispose();
                }
            }

            foreach (var lazyConstructor in this.BlobContextInstances.Values)
            {
                if (lazyConstructor.IsValueCreated)
                {
                    lazyConstructor.Value.Dispose();
                }
            }

            this.QueueContextInstances.Clear();
            this.BlobContextInstances.Clear();
        }
    }
}
