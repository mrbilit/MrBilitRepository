using MrBilit.Repository.Abstractions;

namespace MrBilit.Repository.Caching.MemoryCache;

public class MemoryListCacheProvider<T> : INonSynchronizedCacheProvider<T>, IListCacheProvider<T> where T : class
{
    private static List<T>? _values = null;

    public ValueTask<IEnumerable<T>> GetListAsync()
    {
        if (_values == null) throw new InvalidOperationException("Memory list cache is not initialized yet");
        return ValueTask.FromResult(_values.AsEnumerable());
    }

    public ValueTask InitAsync(IEnumerable<T> values)
    {
        _values = values.ToList();
        return ValueTask.CompletedTask;
    }

    public async ValueTask ResyncAsync(IEnumerable<T> values)
    {
        await InitAsync(values);
    }
}
