using System.Linq.Expressions;
using System.Reflection;

using Ardalis.Specification;

namespace SpecificationPOC.Specification.Base;

public static class SpecificationUtils
{
    internal static PropertyInfo GetPropertyInfo<TSource, TProperty>(this Expression<Func<TSource, TProperty>> propertyLambda)
    {
        if (propertyLambda.Body is not MemberExpression member)
        {
            throw new ArgumentException(string.Format(
                "Expression '{0}' refers to a method, not a property.",
                propertyLambda.ToString()));
        }

        if (member.Member is not PropertyInfo propInfo)
        {
            throw new ArgumentException(string.Format(
                "Expression '{0}' refers to a field, not a property.",
                propertyLambda.ToString()));
        }

        Type type = typeof(TSource);
        if (propInfo.ReflectedType != null && type != propInfo.ReflectedType && !type.IsSubclassOf(propInfo.ReflectedType))
        {
            throw new ArgumentException(string.Format(
                "Expression '{0}' refers to a property that is not from type {1}.",
                propertyLambda.ToString(),
                type));
        }

        return propInfo;
    }

    public static List<string> ConvertToStringIncludes<T>(this ISpecification<T> specification)
    {
        if (!specification.IncludeExpressions.Any())
        {
            return new List<string>();
        }
        List<string> result = new List<string>();
        List<string> include = new List<string>();
        foreach (var expression in specification.IncludeExpressions)
        {
            if (expression.PreviousPropertyType is null)
            {
                if (include.Any())
                {
                    result.Add(string.Join('.', include));
                    include = new List<string>();
                }
            }
            include.Add(expression.LambdaExpression.Body.ToString().Substring(expression.LambdaExpression.Body.ToString().IndexOf('.') + 1));
        }
        if (include.Any())
            result.Add(string.Join('.', include));

        return result;

    }


}

