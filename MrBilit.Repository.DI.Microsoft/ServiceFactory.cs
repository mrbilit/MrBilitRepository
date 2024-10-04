using Microsoft.Extensions.DependencyInjection;

namespace MrBilit.Repository.DI.Microsoft;

public class ServiceFactory : IServiceFactory
{
    private readonly IServiceProvider _serviceProvider;

    public ServiceFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public T? InstantiateByType<T>(Type type) where T : class
    {
        var typeInstance = ActivatorUtilities.CreateInstance(_serviceProvider, type) as T;
        return typeInstance;
    }

    public void InstantiateByTypeAndRunScoped<T>(Type type, Action<T> action) where T : class
    {
        using var scope = _serviceProvider.CreateScope();
        var typeInstance = ActivatorUtilities.CreateInstance(scope.ServiceProvider, type) as T;
        if (typeInstance == null) return;
        action(typeInstance);
    }

    public void InstantiateByTypeAndRunScoped<T>(Action<T> action) where T : class
        => InstantiateByTypeAndRunScoped(typeof(T), action);

    public async ValueTask InstantiateByTypeAndRunScoped<T>(Type type, Func<T, ValueTask> action) where T : class
    {
        using var scope = _serviceProvider.CreateScope();
        var typeInstance = ActivatorUtilities.CreateInstance(scope.ServiceProvider, type) as T;
        if (typeInstance == null) return;
        await action(typeInstance);
    }

    public ValueTask InstantiateByTypeAndRunScoped<T>(Func<T, ValueTask> action) where T : class
        => InstantiateByTypeAndRunScoped(typeof(T), action);
}
