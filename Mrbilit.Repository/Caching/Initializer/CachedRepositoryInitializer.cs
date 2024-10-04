namespace MrBilit.Repository.Caching.Initializer;

public class CachedRepositoryInitializer : ICachedRepositoryInitializer
{
    private readonly IServiceFactory serviceFactory;

    public CachedRepositoryInitializer(IServiceFactory serviceFactory)
    {
        this.serviceFactory = serviceFactory;
    }

    public async Task InitAllRepositoriesOfAssemblyContaining<T>()
    {
        var assembly = typeof(T).Assembly;
        var allCachedRepos = assembly.GetTypes().Where(t => IsSubclassOfRawGeneric(typeof(CachedRepository<>), t));
        foreach (var repo in allCachedRepos)
        {
            var svc = serviceFactory.InstantiateByType<IInitializable>(repo);
            if (svc == null) continue;
            await svc.InitAsync();
        }
    }

    private static bool IsSubclassOfRawGeneric(Type generic, Type toCheck)
    {
        while (toCheck != null && toCheck != typeof(object))
        {
            var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
            if (generic == cur)
            {
                return true;
            }
            toCheck = toCheck.BaseType;
        }
        return false;
    }
}
