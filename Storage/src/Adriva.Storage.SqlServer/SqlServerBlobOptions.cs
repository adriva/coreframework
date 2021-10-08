namespace Adriva.Storage.SqlServer
{
    public class SqlServerBlobOptions : ISqlServerModelOptions
    {
        public string ConnectionString { get; set; }

        public string SchemaName { get; set; } = "dbo";

        public string TableName { get; set; } = "Queue";

        public string RetrieveProcedureName { get; set; } = "RetrieveQueueMessage";
    }
}
