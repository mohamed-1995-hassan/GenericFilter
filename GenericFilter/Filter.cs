
using System.Linq.Expressions;
using System.Reflection;

namespace GenericFilter
{
    public static class Filter
    {
        public static async Task<Expression<Func<T, bool>>?> DynamicFilter<T>(this IFilter filterCreteria)
        {
            var entityPropertyInfo = typeof(T).GetProperties().ToList();
            Expression<Func<T, bool>>? criteria = default;
            var dtoPropertyInfos = filterCreteria.GetType().GetProperties()
                                                 .Where(p => p.GetValue(filterCreteria) != null)
                                                .ToList();
            var parameter = Expression.Parameter(typeof(T), "p");

            foreach (var propertyInfo in dtoPropertyInfos)
            {
                Expression<Func<T, bool>>? predicate = default;
                PropertyInfo? property = entityPropertyInfo.FirstOrDefault(p => p.Name == propertyInfo.Name);

                if (property != null)
                {
                    var constValue = Expression.Constant(propertyInfo.GetValue(filterCreteria), propertyInfo.PropertyType);
                    MemberExpression propertyExpression = Expression.Property(parameter, property);

                    if (propertyInfo.GetCustomAttribute<StringComparisonAttribute>() != null)
                    {
                        MethodInfo? method = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                        var containsMethodExp = Expression.Call(propertyExpression, method, new Expression[] { constValue });
                        predicate = Expression.Lambda<Func<T, bool>>(containsMethodExp, parameter);
                    }

                    else if (propertyInfo.GetCustomAttribute<GreaterThanAttribute>() != null)
                    {
                        BinaryExpression graterBinaryExpression = Expression.GreaterThanOrEqual(propertyExpression, constValue);
                        predicate = Expression.Lambda<Func<T, bool>>(graterBinaryExpression, parameter);
                    }
                    else if (propertyInfo.GetCustomAttribute<LessThanAttribute>() != null)
                    {
                        BinaryExpression LessThanBinaryExpression = Expression.LessThanOrEqual(propertyExpression, constValue);
                        predicate = Expression.Lambda<Func<T, bool>>(LessThanBinaryExpression, parameter);
                    }
                    else if (propertyInfo.GetCustomAttribute<EqualAttribute>() != null)
                    {
                        predicate = Expression.Lambda<Func<T, bool>>(
                        Expression.Equal(Expression.PropertyOrField(parameter, propertyInfo.Name),
                        Expression.Constant(propertyInfo.GetValue(filterCreteria), property.PropertyType)),
                        parameter);
                    }

                    if (predicate == default)
                        continue;
                    if (criteria == null)
                        criteria = predicate;
                    else
                    {
                        var combined = Expression.AndAlso(criteria.Body, predicate.Body);
                        criteria = Expression.Lambda<Func<T, bool>>(combined, parameter);
                    }
                }
            }
            return criteria;
        }
    }
}
