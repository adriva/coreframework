namespace Adriva.Storage.Abstractions
{
    public interface ITableItem
    {
        string PartitionKey { get; set; }

        string RowKey { get; set; }
    }
}