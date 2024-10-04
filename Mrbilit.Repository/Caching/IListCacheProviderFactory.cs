using MrBilit.Repository.Abstractions;

namespace MrBilit.Repository.Caching;

public interface IListCacheProviderFactory
{
    public IListCacheProvider<TValue> Create<TValue>() where TValue : class;
}
