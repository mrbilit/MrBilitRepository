using System.Collections.ObjectModel;
using System.Linq.Expressions;

using SpecificationPOC.Specification.Base;

namespace MrBilit.Repository.Caching.Include;

public abstract class IncludeSpecification<T> where T : class
{
    // Include
    public ReadOnlyCollection<string> Includes => _includes.AsReadOnly();
    private List<string> _includes = new List<string>();
    protected IncludedProperty<T, TProperty> AddInclude<TProperty>(Expression<Func<T, TProperty>> navigationPropertyPath)
        where TProperty : class
    {
        var propName = navigationPropertyPath?.GetPropertyInfo()?.Name;
        AddInclude(propName);
        return new IncludedProperty<T, TProperty>(propName, this);
    }
    public void AddInclude(string path)
    {
        _includes.Add(path);
    }

    public bool ContainIncludes(IEnumerable<string> includes)
    {
        if (includes == null || !includes.Any()) return true;

        if (includes.Any(p => !_includes.Any(q => q.StartsWith(p))))
            return false;
        return true;
    }
}

