﻿
using System.Linq.Expressions;
using System.Reflection;

namespace GenericFilter
{
    public static class Filter
    {
        public static Expression<Func<T, bool>>? DynamicFilter<T>(this IFilter filterCreteria)
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
                    var stringComparison = propertyInfo.GetCustomAttribute<StringComparisonAttribute>();
                    var equalComparison = propertyInfo.GetCustomAttribute<EqualAttribute>();

                    if (stringComparison != null)
                        predicate = stringComparison.BuildExpression<T>(propertyExpression, constValue);

                    else if (propertyInfo.GetValue(filterCreteria) is IMinMaxFilter minMaxFilter)
                    {
                        if (minMaxFilter != null)
                            predicate = minMaxFilter.BuildMinMax<T>(propertyExpression);
                    }

                    else if (equalComparison != null)
                    {
                        predicate = equalComparison.BuildExpression<T>(propertyInfo, property, filterCreteria);
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
