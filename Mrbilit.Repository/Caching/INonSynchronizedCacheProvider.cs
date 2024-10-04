namespace MrBilit.Repository.Caching;

public interface INonSynchronizedCacheProvider<T>
{
    public ValueTask ResyncAsync(IEnumerable<T> values);
}
