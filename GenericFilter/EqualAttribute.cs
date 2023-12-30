
using System.Linq.Expressions;
using System.Reflection;

namespace GenericFilter
{
    [AttributeUsage(AttributeTargets.Property)]
    public class EqualAttribute : Attribute
    {
        public Expression<Func<TEntity, bool>>? BuildExpression<TEntity>(PropertyInfo propertyInfo, PropertyInfo property, object filterCreteria)
        {
            var parameter = Expression.Parameter(typeof(TEntity), "p");
            return Expression.Lambda<Func<TEntity, bool>>(
                        Expression.Equal(Expression.PropertyOrField(parameter, propertyInfo.Name),
                        Expression.Constant(propertyInfo.GetValue(filterCreteria), property.PropertyType)),
                        parameter);
        }
    }
}
