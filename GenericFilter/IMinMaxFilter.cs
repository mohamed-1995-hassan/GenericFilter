
using System.Linq.Expressions;

namespace GenericFilter
{
    public interface IMinMaxFilter
    {
        public Expression<Func<T, bool>>? BuildMinMax<T>(MemberExpression memberExpression);
    }
}
