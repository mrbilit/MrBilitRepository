
namespace MrBilit.Repository.Caching.Initializer;

public interface ICachedRepositoryInitializer
{
    Task InitAllRepositoriesOfAssemblyContaining<T>();
}