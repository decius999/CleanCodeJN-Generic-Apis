using System.Linq.Expressions;

namespace CleanCodeJN.GenericApis.Extensions;

public static class EFQueryExtensions
{
    public static IOrderedQueryable<TEntity> OrderByString<TEntity>(this IQueryable<TEntity> source, string orderByProperty, bool desc)
    {
        var type = typeof(TEntity);
        var command = desc ? "OrderByDescending" : "OrderBy";

        orderByProperty = orderByProperty == "null" || string.IsNullOrWhiteSpace(orderByProperty) ? "Id" : orderByProperty;
        orderByProperty = orderByProperty[0].ToString().ToUpper(System.Globalization.CultureInfo.CurrentCulture) + orderByProperty[1..];

        var property = type.GetProperty(orderByProperty);
        var parameter = Expression.Parameter(type, "p");
        var propertyAccess = Expression.MakeMemberAccess(parameter, property);
        var orderByExpression = Expression.Lambda(propertyAccess, parameter);
        var resultExpression = Expression.Call(typeof(Queryable), command, [type, property.PropertyType], source.Expression, Expression.Quote(orderByExpression));

        return (IOrderedQueryable<TEntity>)source.Provider.CreateQuery<TEntity>(resultExpression);
    }

    public static IOrderedQueryable<TEntity> OrderByString<TEntity>(this IQueryable<TEntity> source, string orderByProperty, string ascDesc) => source.OrderByString(orderByProperty, ascDesc == "-1");
}
