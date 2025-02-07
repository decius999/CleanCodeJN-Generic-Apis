using System.Linq.Expressions;
using System.Reflection;
using CleanCodeJN.GenericApis.Abstractions.Models;
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

        var properties = orderByProperty.Split('_');
        var parameter = Expression.Parameter(type, "p");
        Expression propertyAccess = parameter;
        PropertyInfo property = null;
        foreach (var innerProperty in properties)
        {
            property = propertyAccess.Type.GetProperty(innerProperty);
            propertyAccess = Expression.MakeMemberAccess(propertyAccess, property);
        }

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
            MethodInfo filterMethod = null;
            ConstantExpression filterExpression = null;
            PropertyInfo property = null;

            if (innerFilter.Type == FilterTypeEnum.STRING)
            {
                filterMethod = typeof(string).GetMethod("Contains", [typeof(string)]);
                filterExpression = Expression.Constant(innerFilter.Value, typeof(string));

                ExecuteFilter<TEntity, TKey>(filter, parameter, ref finalExpression, innerFilter, filterMethod, filterExpression, ref property);
            }
            else if (innerFilter.Type is FilterTypeEnum.INTEGER or FilterTypeEnum.INTEGER_NULLABLE)
            {
                filterMethod = typeof(int).GetMethod("Equals", new[] { typeof(int) });
                filterExpression = Expression.Constant(Convert.ToInt32(innerFilter.Value), typeof(int));

                ExecuteFilter<TEntity, TKey>(filter, parameter, ref finalExpression, innerFilter, filterMethod, filterExpression, ref property);
            }
            else if (innerFilter.Type is FilterTypeEnum.DATETIME or FilterTypeEnum.DATETIME_NULLABLE)
            {
                filterMethod = typeof(DateTime).GetMethod("Equals", new[] { typeof(DateTime) });
                filterExpression = Expression.Constant(DateTime.Parse(innerFilter.Value), typeof(DateTime));

                ExecuteFilter<TEntity, TKey>(filter, parameter, ref finalExpression, innerFilter, filterMethod, filterExpression, ref property);
            }
            else
            {
                ExecuteFilter<TEntity, TKey>(filter, parameter, ref finalExpression, innerFilter, filterMethod, filterExpression, ref property);
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

    private static void ExecuteFilter<TEntity, TKey>(SearchFilter filter, ParameterExpression parameter, ref Expression finalExpression, FilterValue innerFilter, MethodInfo filterMethod, ConstantExpression filterExpression, ref PropertyInfo property) where TEntity : class, IEntity<TKey>
    {
        var properties = innerFilter.Field.Split('_');
        Expression propertyAccess = parameter;
        foreach (var innerProperty in properties)
        {
            property = propertyAccess.Type.GetProperty(innerProperty);
            propertyAccess = Expression.MakeMemberAccess(propertyAccess, property);
        }

        var containsExpression = Expression.Call(propertyAccess, filterMethod, filterExpression);
        var lambdaExpression = Expression.Lambda<Func<TEntity, bool>>(containsExpression, parameter);

        if (filter.Condition == FilterTypeConditionEnum.AND)
        {
            finalExpression = finalExpression == null ? lambdaExpression.Body : Expression.AndAlso(finalExpression, lambdaExpression.Body);
        }
        else if (filter.Condition == FilterTypeConditionEnum.OR)
        {
            finalExpression = finalExpression == null ? lambdaExpression.Body : Expression.OrElse(finalExpression, lambdaExpression.Body);
        }
    }
}
