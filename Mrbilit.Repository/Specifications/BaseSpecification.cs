using Ardalis.Specification;

namespace Mrbilit.Repository.Specifications;

public abstract class BaseSpecification<T> : Specification<T> where T : class
{
}

public abstract class BaseSpecification<T, TResult> : Specification<T, TResult> where T : class where TResult : class
{

}

