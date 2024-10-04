using MrBilit.Repository.Abstractions;

namespace MrBilit.Repository.Caching;

public interface IMapCacheProviderFactory
{
    public IMapCacheProvider<TValue> Create<TValue>(Func<TValue, string> keySelector) where TValue : class;
}
