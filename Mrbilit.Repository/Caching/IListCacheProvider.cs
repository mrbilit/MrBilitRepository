namespace MrBilit.Repository.Abstractions;

public interface IListCacheProvider<T> where T : class
{
    public ValueTask InitAsync(IEnumerable<T> values);
    public ValueTask<IEnumerable<T>> GetListAsync();
}
