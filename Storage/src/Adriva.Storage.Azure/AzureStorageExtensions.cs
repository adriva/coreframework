using System;
using Adriva.Storage.Abstractions;
using Adriva.Storage.Azure;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class AzureStorageExtensions
    {
        public static IStorageBuilder AddAzureBlob(this IStorageBuilder storageBuilder, ServiceLifetime serviceLifetime, Action<AzureBlobConfiguration> configure)
        {

            return storageBuilder.AddAzureBlob(Options.Options.DefaultName, serviceLifetime, configure);
        }

        public static IStorageBuilder AddAzureBlob(this IStorageBuilder storageBuilder, string name, ServiceLifetime serviceLifetime, Action<AzureBlobConfiguration> configure)
        {
            storageBuilder.AddBlobClient<AzureBlobClient>(name, serviceLifetime).Configure(configure);
            return storageBuilder;
        }

        public static IStorageBuilder AddAzureQueue(this IStorageBuilder storageBuilder, ServiceLifetime serviceLifetime, Action<AzureQueueConfiguration> configure)
        {
            return storageBuilder.AddAzureQueue(Options.Options.DefaultName, serviceLifetime, configure);
        }

        public static IStorageBuilder AddAzureQueue(this IStorageBuilder storageBuilder, string name, ServiceLifetime serviceLifetime, Action<AzureQueueConfiguration> configure)
        {
            storageBuilder.AddQueueClient<AzureQueueClient>(name, serviceLifetime).Configure(configure);
            return storageBuilder;
        }

        public static IStorageBuilder AddAzureTable(this IStorageBuilder storageBuilder, ServiceLifetime serviceLifetime, Action<AzureTableConfiguration> configure)
        {
            return storageBuilder.AddAzureTable(Options.Options.DefaultName, serviceLifetime, configure);
        }

        public static IStorageBuilder AddAzureTable(this IStorageBuilder storageBuilder, string name, ServiceLifetime serviceLifetime, Action<AzureTableConfiguration> configure)
        {
            return storageBuilder.AddAzureTable<TableEntityBuilder>(name, serviceLifetime, configure);
        }

        public static IStorageBuilder AddAzureTable<TTableEntityBuilder>(this IStorageBuilder storageBuilder, string name, ServiceLifetime serviceLifetime, Action<AzureTableConfiguration> configure) where TTableEntityBuilder : class, ITableEntityBuilder
        {
            storageBuilder.AddTableClient<AzureTableClient>(name, serviceLifetime).Configure(configure);
            storageBuilder.Services.AddSingleton<ITableEntityBuilder, TTableEntityBuilder>();
            return storageBuilder;
        }
    }
}