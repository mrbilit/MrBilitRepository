using Ardalis.Specification;

namespace MrBilit.Repository.Abstractions;

public interface IRepository<T> : Ardalis.Specification.IRepositoryBase<T> where T : class
{
    T? FirstOrDefault(ISpecification<T> specification);
    List<T> List();
    List<T> List(ISpecification<T> specification);
    TResult? FirstOrDefault<TResult>(ISpecification<T, TResult> specification);
}
