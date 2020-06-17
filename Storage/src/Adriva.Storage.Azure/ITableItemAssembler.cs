using Adriva.Storage.Abstractions;
using Microsoft.Azure.Cosmos.Table;

namespace Adriva.Storage.Azure
{
    public interface ITableItemAssembler
    {
        TItem Assemble<TItem>(DynamicTableEntity tableEntity) where TItem : class, ITableItem, new();

        ITableEntity Disassemble<TItem>(TItem item) where TItem : class, ITableItem;
    }
}
