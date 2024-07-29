namespace Adriva.Extensions.Caching.Abstractions
{
    internal sealed class CacheWrapper<TCache> : ICache<TCache> where TCache : ICache
    {
        public TCache Instance { get; private set; }

        internal CacheWrapper(TCache instance)
        {
            this.Instance = instance;
        }
    }
}
