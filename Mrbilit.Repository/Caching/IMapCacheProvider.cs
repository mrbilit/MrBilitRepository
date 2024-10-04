namespace MrBilit.Repository.Abstractions;

public interface IMapCacheProvider<T> where T : class
{
    public ValueTask InitAsync(IEnumerable<T> values);
    public ValueTask<T?> GetValueOrDefaultAsync(string key);
}
