using Mrbilit.Repository.Common.Extensions;

using MrBilit.Repository;

namespace Mrbilit.Repository.Initializer;

public class RepositoryInitializer : IRepositoryInitializer
{
    private readonly IServiceFactory serviceFactory;
    private readonly IDatabaseUtility? _databaseUtility;

    public RepositoryInitializer(IServiceFactory serviceFactory, IDatabaseUtility? databaseUtility)
    {
        this.serviceFactory = serviceFactory;
        _databaseUtility = databaseUtility;
    }

    public void InitAllRepositoriesOfAssemblyContaining<T>()
    {
        var assembly = typeof(T).Assembly;
        var allCachedRepos = assembly.GetTypes().Where(t => TypeExtensions.IsSubclassOfRawGeneric(typeof(CachedRepository<>), t));
        foreach (var repo in allCachedRepos)
        {
            var svc = serviceFactory.InstantiateByType<IInitializable>(repo);
            if (svc == null) continue;
            svc.InitAsync().GetAwaiter().GetResult();
        }

        if (_databaseUtility != null)
        {
            _databaseUtility.Init(assembly);
        }


    }
}
