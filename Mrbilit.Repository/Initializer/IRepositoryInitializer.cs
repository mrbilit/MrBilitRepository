namespace Mrbilit.Repository.Initializer;

public interface IRepositoryInitializer
{
    void InitAllRepositoriesOfAssemblyContaining<T>();
}