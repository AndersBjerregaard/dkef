using System.Linq.Expressions;
using System.Reflection;

namespace Dkef.Services;

public static class SortOrderService
{
    public static Expression<Func<T, object>> GetKeySelector<T>(string propertyName) where T : class
    {
        Type type = typeof(T);

        PropertyInfo? propertyInfo = type.GetProperty(propertyName);

        if (propertyInfo == null)
        {
            throw new ArgumentException($"Property '{propertyName}' not found on type '{type.Name}'.");
        }

        ParameterExpression parameter = Expression.Parameter(type, "x");

        MemberExpression propertyAccess = Expression.Property(parameter, propertyInfo);

        LambdaExpression lambda = Expression.Lambda(
            typeof(Func<T, object>), // The delegate type for the expression
            propertyAccess,
            parameter
        );

        return (Expression<Func<T, object>>)lambda;
    }
}