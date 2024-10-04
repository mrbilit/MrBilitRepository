namespace MrBilit.Repository.Abstractions;

public interface IRepository<T> : Ardalis.Specification.IRepositoryBase<T> where T : class
{
}
