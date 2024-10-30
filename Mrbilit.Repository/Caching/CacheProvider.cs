using MrBilit.Repository.Abstractions;

namespace MrBilit.Repository.Caching;

public class CacheProvider<T> : ICacheProvider<T> where T : class
{
    private IListCacheProvider<T>? _listCacheProvider = null;
    private static Dictionary<string, IMapCacheProvider<T>> _mapCacheProvider = new();

    private readonly IListCacheProviderFactory _listCacheProviderFactory;
    private readonly IMapCacheProviderFactory _mapCacheProviderFactory;

    public CacheProvider(IListCacheProviderFactory listCacheProviderFactory, IMapCacheProviderFactory mapCacheProviderFactory)
    {
        _listCacheProviderFactory = listCacheProviderFactory;
        _mapCacheProviderFactory = mapCacheProviderFactory;
    }

    public async Task InitAsync(IEnumerable<T> values)
    {
        if (values is null)
        {
            throw new ArgumentNullException(nameof(values));
        }

        if (_listCacheProvider != null)
        {
            await _listCacheProvider.InitAsync(values);
        }

        foreach (var cache in _mapCacheProvider)
        {
            await cache.Value.InitAsync(values);
        }
    }

    public void EnableListCache()
    {
        _listCacheProvider = _listCacheProviderFactory.Create<T>();
    }

    public void AddMapCache(string name, Func<T, string> keySelector)
    {
        if (name is null)
        {
            throw new ArgumentNullException(nameof(name));
        }

        if (!_mapCacheProvider.ContainsKey(name))
            _mapCacheProvider.Add(name, _mapCacheProviderFactory.Create(keySelector));
    }

    public bool ListCacheEnabled => _listCacheProvider != null;

    public ValueTask<IEnumerable<T>> GetListAsync()
    {
        if (_listCacheProvider == null)
        {
            throw new InvalidOperationException("List cache in not enabled on this repository.");
        }

        return _listCacheProvider.GetListAsync();
    }

    public ValueTask<T?> GetByKeyAsync(string mapName, string key)
    {
        if (mapName is null)
        {
            throw new ArgumentNullException(nameof(mapName));
        }

        if (key is null)
        {
            throw new ArgumentNullException(nameof(key));
        }

        var provider = _mapCacheProvider.GetValueOrDefault(mapName);
        if (provider == null)
        {
            throw new InvalidOperationException("Cache is not configured with name = " + mapName);
        }

        return provider.GetValueOrDefaultAsync(key);
    }

    public async ValueTask ResyncAsync(IEnumerable<T> values)
    {
        if (_listCacheProvider is INonSynchronizedCacheProvider<T> listProvider)
        {
            await listProvider.ResyncAsync(values);
        }

        foreach (var cache in _mapCacheProvider)
        {
            if (cache.Value is INonSynchronizedCacheProvider<T> mapProvider)
            {
                await mapProvider.ResyncAsync(values);
            }
        }
    }
}
