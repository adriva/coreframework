namespace Adriva.Storage.Azure
{
    public interface ITableEntityMapperFactory
    {
        ITableEntityMapper<T> GetMapper<T>() where T : class;
    }
}
