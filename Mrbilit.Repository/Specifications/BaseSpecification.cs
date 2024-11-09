using Ardalis.Specification;

namespace Mrbilit.Repository.Specifications;

public class BaseSpecification<T> : Specification<T> where T : class
{
}

public class BaseSpecification<T, TResult> : Specification<T, TResult> where T : class
{

}

