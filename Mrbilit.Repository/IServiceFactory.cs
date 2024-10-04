namespace MrBilit.Repository;

public interface IServiceFactory
{
    public T? InstantiateByType<T>(Type type) where T : class;
    public void InstantiateByTypeAndRunScoped<T>(Type type, Action<T> action) where T : class;
    public ValueTask InstantiateByTypeAndRunScoped<T>(Type type, Func<T, ValueTask> action) where T : class;
    public void InstantiateByTypeAndRunScoped<T>(Action<T> action) where T : class;
    public ValueTask InstantiateByTypeAndRunScoped<T>(Func<T, ValueTask> action) where T : class;
}
