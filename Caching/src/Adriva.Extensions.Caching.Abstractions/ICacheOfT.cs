namespace Adriva.Extensions.Caching.Abstractions
{
    public interface ICache<TCache> where TCache : ICache
    {
        TCache Instance { get; }
    }
}
