using MrBilit.Repository.Caching.Include;

namespace SpecificationPOC.Specification.Base;

public interface IIncludedProperty<TEntity, out TProperty>
    where TProperty : class
    where TEntity : class
{
    public string Path { get; }
    public IncludeSpecification<TEntity> Parent { get; }
}