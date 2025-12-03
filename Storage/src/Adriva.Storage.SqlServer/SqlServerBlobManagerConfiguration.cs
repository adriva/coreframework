namespace Adriva.Storage.SqlServer;

public record SqlServerBlobManagerConfiguration
{
    public required string ConnectionString { get; set; }

    public required string SchemaName { get; set; } = "dbo";

    public required string TableName { get; set; } = "FileContent";

    public required string FileGroupName { get; set; }

    public required string UpsertProcedureName { get; set; }

    public bool CreateDatabaseObjectsIfNeeded { get; set; }

    public bool AutoResolveMimeType { get; set; } = true;
}
