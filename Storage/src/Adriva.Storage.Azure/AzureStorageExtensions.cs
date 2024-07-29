using System;
using Adriva.Storage.Azure;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class AzureStorageExtensions
    {
        public static IStorageBuilder AddAzureTable(this IStorageBuilder builder, string name, Action<AzureTableClientOptions> configure)
        {
            return builder.AddAzureTable<TableEntityMapperFactory>(name, configure);
        }

        public static IStorageBuilder AddAzureTable<TMapperFactory>(this IStorageBuilder builder, string name, Action<AzureTableClientOptions> configure) where TMapperFactory : class, ITableEntityMapperFactory
        {
            builder.Services.TryAddSingleton<ITableEntityMapperFactory, TMapperFactory>();
            builder.AddTableClient<Adriva.Storage.Azure.TableClient, AzureTableClientOptions>(name, configure);
            return builder;
        }

        public static IStorageBuilder AddAzureBlob(this IStorageBuilder builder, string name, Action<AzureBlobClientOptions> configure)
        {
            builder.AddBlobClient<Adriva.Storage.Azure.BlobClient, AzureBlobClientOptions>(name, configure);
            return builder;
        }
    }
}