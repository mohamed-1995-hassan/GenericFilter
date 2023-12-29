
using System;
using System.Linq.Expressions;
using System.Reflection.Metadata;

namespace GenericFilter
{
    public class MinMax<T> : IMinMaxFilter where T : struct
    {
        public T? Max { get; set; }
        public T? Min { get; set; }

        public Expression<Func<TEntity, bool>>? BuildMinMax<TEntity>(MemberExpression memberExpression)
        {
            var parameter = Expression.Parameter(typeof(TEntity), "p");
            BinaryExpression? min = default;
            BinaryExpression? max = default;

            if (Max.HasValue)
            {
                var constValue = Expression.Constant(Max.Value, Max.Value.GetType());
                max = Expression.GreaterThanOrEqual(memberExpression, constValue);
            }

            if (Min.HasValue)
            {
                var constValue = Expression.Constant(Min.Value, Min.GetType());
                min = Expression.LessThanOrEqual(memberExpression, constValue);
            }

            if (min == default)
                return Expression.Lambda<Func<TEntity, bool>>(max, parameter);
            if (max == default)
                return Expression.Lambda<Func<TEntity, bool>>(min, parameter);

            if (min == default && max == default)
                return default;

            var combined = Expression.AndAlso(max, min);
            var predicate = Expression.Lambda<Func<TEntity, bool>>(combined, parameter);
            return predicate;
        }
    }
}
