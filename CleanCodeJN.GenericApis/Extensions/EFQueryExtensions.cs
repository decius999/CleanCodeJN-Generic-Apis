using System.Linq.Expressions;
using System.Reflection;
using CleanCodeJN.GenericApis.Models;
using CleanCodeJN.Repository.EntityFramework.Contracts;

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

    public static IQueryable<TEntity> WhereColumnsContainFilter<TEntity, TKey>(this IQueryable<TEntity> source, SearchFilter filter)
        where TEntity : class, IEntity<TKey>
    {
        if (filter == null || !filter.Filters.Any())
        {
            return source;
        }

        var type = typeof(TEntity);
        var parameter = Expression.Parameter(type, "p");
        Expression finalExpression = null;

        foreach (var innerFilter in filter.Filters)
        {
            MethodInfo containsMethod = null;
            ConstantExpression filterExpression = null;
            PropertyInfo property = null;

            if (innerFilter.Type == FilterTypeEnum.STRING)
            {
                containsMethod = typeof(string).GetMethod("Contains", [typeof(string)]);
                filterExpression = Expression.Constant(innerFilter.Value, typeof(string));

                property = type.GetProperty(innerFilter.Field);
                var propertyAccess = Expression.Property(parameter, property);
                var containsExpression = Expression.Call(propertyAccess, containsMethod, filterExpression);
                var lambdaExpression = Expression.Lambda<Func<TEntity, bool>>(containsExpression, parameter);

                finalExpression = finalExpression == null ? lambdaExpression.Body : Expression.AndAlso(finalExpression, lambdaExpression.Body);
            }
            else
            {
                property = type.GetProperty(innerFilter.Field);
                if (property is null)
                {
                    continue;
                }

                var propertyAccess = Expression.Property(parameter, property);
                filterExpression = Expression.Constant(FilterTypeEnumExtensions.ConvertTo(innerFilter), property.PropertyType);
                var equalsExpression = Expression.Equal(propertyAccess, filterExpression);
                var lambdaExpression = Expression.Lambda<Func<TEntity, bool>>(equalsExpression, parameter);

                finalExpression = finalExpression == null ? lambdaExpression.Body : Expression.AndAlso(finalExpression, lambdaExpression.Body);
            }
        }

        if (finalExpression != null)
        {
            var finalLambda = Expression.Lambda<Func<TEntity, bool>>(finalExpression, parameter);

            return source.Where(finalLambda);
        }

        return source;
    }

    public static IQueryable<TEntity> PagedResultList<TEntity, TKey>(
     this IQueryable<TEntity> source,
     int page,
     int pageSize,
     string sortBy,
     string direction,
     SearchFilter filter)
         where TEntity : class, IEntity<TKey> => source
               .WhereColumnsContainFilter<TEntity, TKey>(filter)
               .OrderByString(sortBy, direction)
               .Skip(page * pageSize)
               .Take(pageSize);
}
