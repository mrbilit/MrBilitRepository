using System.Linq.Expressions;

namespace SpecificationPOC.Specification.Base;

public static class IncludedPropertyUtils
{
    public static IIncludedProperty<TEntity, TNestedProperty> ThenInclude<TEntity, TProperty, TNestedProperty>(this IIncludedProperty<TEntity, IEnumerable<TProperty>> source, Expression<Func<TProperty, TNestedProperty>> navigationPropertyPath)
        where TNestedProperty : class
        where TProperty : class
        where TEntity : class
    {
        var propName = source.Path + "." + navigationPropertyPath?.GetPropertyInfo()?.Name;
        source.Parent.AddInclude(propName);
        return new IncludedProperty<TEntity, TNestedProperty>(propName, source.Parent);
    }

    public static IncludedProperty<TEntity, TNestedProperty> ThenInclude<TEntity, TProperty, TNestedProperty>(this IncludedProperty<TEntity, TProperty> source, Expression<Func<TProperty, TNestedProperty>> navigationPropertyPath)
        where TNestedProperty : class
        where TProperty : class
        where TEntity : class
    {
        var propName = source.Path + "." + navigationPropertyPath?.GetPropertyInfo()?.Name;
        source.Parent.AddInclude(propName);
        return new IncludedProperty<TEntity, TNestedProperty>(propName, source.Parent);
    }
}