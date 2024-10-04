namespace MrBilit.Repository.Caching;

public interface ICacheProvider<T> where T : class
{
    public void EnableListCache();
    public void AddMapCache(string name, Func<T, string> keySelector);
    public Task InitAsync(IEnumerable<T> values);
    public bool ListCacheEnabled { get; }
    public ValueTask<IEnumerable<T>> GetListAsync();
    public ValueTask<T?> GetByKeyAsync(string mapName, string key);
    ValueTask ResyncAsync(IEnumerable<T> values);
}
