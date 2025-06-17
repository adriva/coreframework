using Azure.Data.Tables;

namespace Adriva.Storage.Azure
{
    public interface ITableEntityMapper<T> where T : class
    {
        T Build(TableEntity tableEntity);

        TableEntity Build(T tableEntity);
    }
}
