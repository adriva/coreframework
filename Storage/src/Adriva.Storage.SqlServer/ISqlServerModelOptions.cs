namespace Adriva.Storage.SqlServer
{
    public interface ISqlServerModelOptions
    {
        string ConnectionString { get; set; }

        string SchemaName { get; set; }

        string TableName { get; set; }

        string RetrieveProcedureName { get; set; }
    }
}
