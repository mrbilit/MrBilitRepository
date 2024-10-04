using MrBilit.Repository.Abstractions;

namespace MrBilit.Repository.Caching.MemoryCache;

public class MemoryListCacheProviderFactory : IListCacheProviderFactory
{
    public IListCacheProvider<TValue> Create<TValue>() where TValue : class
    {
        return new MemoryListCacheProvider<TValue>();
    }
}
