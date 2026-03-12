using System.Linq.Expressions;
using AutoMapper;
using CleanCodeJN.GenericApis.Abstractions.Contracts;
using CleanCodeJN.GenericApis.Abstractions.Extensions;
using CleanCodeJN.GenericApis.Commands;
using MediatR;

namespace CleanCodeJN.GenericApis.API;

/// <summary>
/// Abstract base class providing GET list endpoint implementations with optional pagination and filtering.
/// </summary>
/// <typeparam name="TEntity">The entity type to retrieve.</typeparam>
/// <typeparam name="TGetDto">The DTO type returned in the response.</typeparam>
public abstract class GetBase<TEntity, TGetDto>(IMediator commandBus, IMapper mapper) : ApiBase(commandBus, mapper)
    where TEntity : class
    where TGetDto : class, IDto
{
    /// <summary>
    /// Gets or sets the list of navigation property includes applied to the query.
    /// </summary>
    public List<Expression<Func<TEntity, object>>> Includes { get; set; }

    /// <summary>
    /// Gets or sets the filter expression applied as a WHERE clause on the query.
    /// </summary>
    public Expression<Func<TEntity, bool>> Where { get; set; }

    /// <summary>
    /// Gets or sets the projection expression applied as a SELECT clause on the query.
    /// </summary>
    public Expression<Func<TEntity, TEntity>> Select { get; set; }

    /// <summary>
    /// Retrieves all entities matching the configured includes and where clause without pagination.
    /// </summary>
    /// <typeparam name="TKey">The type of the entity's primary key.</typeparam>
    /// <param name="asNoTracking">Whether to disable EF change tracking.</param>
    /// <param name="ignoreQueryFilters">Whether to ignore global EF query filters.</param>
    /// <param name="asSplitQuery">Whether to use split queries for includes.</param>
    /// <returns>An <see cref="IResult"/> representing the HTTP response.</returns>
    public virtual async Task<IResult> Get<TKey>(bool asNoTracking = true, bool ignoreQueryFilters = false, bool asSplitQuery = false) =>
        await Handle<TEntity, List<TGetDto>>(new GetRequest<TEntity, TKey>
        {
            Includes = Includes ?? [],
            Where = Where ?? (x => true),
            Select = Select,
            AsNoTracking = asNoTracking,
            IgnoreQueryFilters = ignoreQueryFilters,
            AsSplitQuery = asSplitQuery,
        });

    /// <summary>
    /// Retrieves a paged list of entities sorted by the specified field and direction.
    /// </summary>
    /// <typeparam name="TKey">The type of the entity's primary key.</typeparam>
    /// <param name="page">The page index (0-based).</param>
    /// <param name="pageSize">The number of records per page.</param>
    /// <param name="direction">The sort direction.</param>
    /// <param name="sortBy">The property name to sort by.</param>
    /// <param name="asNoTracking">Whether to disable EF change tracking.</param>
    /// <param name="ignoreQueryFilters">Whether to ignore global EF query filters.</param>
    /// <param name="asSplitQuery">Whether to use split queries for includes.</param>
    /// <returns>An <see cref="IResult"/> representing the HTTP response.</returns>
    public virtual async Task<IResult> Get<TKey>(int page, int pageSize, string direction, string sortBy, bool asNoTracking = true, bool ignoreQueryFilters = false, bool asSplitQuery = false)
       => await Get<TKey>(page, pageSize, direction, sortBy, null, asNoTracking, ignoreQueryFilters, asSplitQuery);

    /// <summary>
    /// Retrieves a filtered and paged list of entities sorted by the specified field and direction.
    /// </summary>
    /// <typeparam name="TKey">The type of the entity's primary key.</typeparam>
    /// <param name="page">The page index (0-based).</param>
    /// <param name="pageSize">The number of records per page.</param>
    /// <param name="direction">The sort direction.</param>
    /// <param name="sortBy">The property name to sort by.</param>
    /// <param name="filter">The filter string applied server-side to column values.</param>
    /// <param name="asNoTracking">Whether to disable EF change tracking.</param>
    /// <param name="ignoreQueryFilters">Whether to ignore global EF query filters.</param>
    /// <param name="asSplitQuery">Whether to use split queries for includes.</param>
    /// <returns>An <see cref="IResult"/> representing the HTTP response.</returns>
    public virtual async Task<IResult> Get<TKey>(int page, int pageSize, string direction, string sortBy, string filter, bool asNoTracking = true, bool ignoreQueryFilters = false, bool asSplitQuery = false)
      => await HandlePagination<TEntity, TGetDto>(new GetRequest<TEntity, TKey>
      {
          Skip = page,
          Take = pageSize,
          SortOrder = direction.GetSortOrder(),
          SortField = sortBy,
          Filter = filter.ToFilter(),
          Includes = Includes ?? [],
          Where = Where ?? (x => true),
          Select = Select,
          AsNoTracking = asNoTracking,
          IgnoreQueryFilters = ignoreQueryFilters,
          AsSplitQuery = asSplitQuery,
      });
}
