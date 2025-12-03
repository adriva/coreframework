using Adriva.Storage.Abstractions;
using Adriva.Storage.SqlServer;
using Microsoft.EntityFrameworkCore;

namespace Microsoft.Extensions.DependencyInjection;

public static class StorageServicesBuilderExtensions
{
    public static IStorageServicesBuilder UseSqlServerBlobs(this IStorageServicesBuilder storageServicesBuilder, SqlServerBlobManagerConfiguration configuration)
    {
        storageServicesBuilder.Services.AddDbContext<SqlServerBlobStorage>(builder =>
        {
            builder.UseSqlServer(configuration.ConnectionString);
            builder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        });

        storageServicesBuilder.Services.AddScoped<SqlServerBlobStorage>();
        storageServicesBuilder.Services.AddBlobManager<SqlServerBlob, SqlServerBlobManager>();

        storageServicesBuilder.Services.Configure<SqlServerBlobManagerConfiguration>(options =>
        {
            options.ConnectionString = configuration.ConnectionString;
            options.CreateDatabaseObjectsIfNeeded = configuration.CreateDatabaseObjectsIfNeeded;
            options.FileGroupName = configuration.FileGroupName;
            options.SchemaName = configuration.SchemaName;
            options.TableName = configuration.TableName;
            options.UpsertProcedureName = configuration.UpsertProcedureName;
        });
        return storageServicesBuilder;
    }
}