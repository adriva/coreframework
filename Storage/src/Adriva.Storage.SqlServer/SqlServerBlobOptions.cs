namespace Adriva.Storage.SqlServer
{
    public class SqlServerBlobOptions : ISqlServerModelOptions
    {
        public string ConnectionString { get; set; }

        public string SchemaName { get; set; } = "dbo";

        public string TableName { get; set; } = "Blob";

        public string RetrieveProcedureName { get; set; }

        public string UpsertProcedureName { get; set; } = "UpsertBlobItem";

        public string UpdateProcedureName { get; set; } = "UpdateBlobItem";
    }
}
