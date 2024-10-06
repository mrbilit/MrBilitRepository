
namespace MrBilit.Repository.Caching.Initializer;

public interface ICachedRepositoryInitializer
{
    void InitAllRepositoriesOfAssemblyContaining<T>();
}