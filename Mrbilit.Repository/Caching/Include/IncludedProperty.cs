using MrBilit.Repository.Caching.Include;

namespace SpecificationPOC.Specification.Base;

public class IncludedProperty<TEntity, TProperty> : IIncludedProperty<TEntity, TProperty>
    where TProperty : class
    where TEntity : class
{
    public string Path { get; private set; }
    public IncludeSpecification<TEntity> Parent { get; private set; }

    public IncludedProperty(string path, IncludeSpecification<TEntity> parent)
    {
        Path = path;
        Parent = parent;
    }
}