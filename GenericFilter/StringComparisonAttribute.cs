
using System;
using System.Linq.Expressions;
using System.Reflection.Metadata;
using System.Reflection;

namespace GenericFilter
{
    [AttributeUsage(AttributeTargets.Property)]
    public class StringComparisonAttribute :Attribute
    {
        public Expression<Func<TEntity, bool>>? BuildExpression<TEntity>(MemberExpression propertyExpression, ConstantExpression constValue)
        {
            var parameter = Expression.Parameter(typeof(TEntity), "p");
            MethodInfo? method = typeof(string).GetMethod("Contains", new[] { typeof(string) });
            var containsMethodExp = Expression.Call(propertyExpression, method, new Expression[] { constValue });
            return Expression.Lambda<Func<TEntity, bool>>(containsMethodExp, parameter);
        }
    }
}
