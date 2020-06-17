using Microsoft.Azure.Cosmos.Table;

namespace Adriva.Storage.Azure
{
    public interface ITableEntityBuilder
    {
        TItem Build<TItem>(DynamicTableEntity tableEntity) where TItem : class, new();
    }
}
