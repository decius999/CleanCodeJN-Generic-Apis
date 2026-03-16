using System.Linq.Expressions;
using CleanCodeJN.GenericApis.Abstractions.Contracts;
using CleanCodeJN.GenericApis.Commands;
using MediatR;

namespace CleanCodeJN.GenericApis.API;

/// <summary>
/// Abstract base class providing a GET-by-ID endpoint implementation for retrieving a single entity by its key.
/// </summary>
/// <typeparam name="TEntity">The entity type to retrieve.</typeparam>
/// <typeparam name="TGetDto">The DTO type returned in the response.</typeparam>
public abstract class GetByIdBase<TEntity, TGetDto>(IMediator commandBus, ICleanCodeMapper mapper) : ApiBase(commandBus, mapper)
    where TEntity : class
    where TGetDto : class, IDto
{
    /// <summary>
    /// Gets or sets the list of navigation property includes applied to the query.
    /// </summary>
    public List<Expression<Func<TEntity, object>>> Includes { get; set; }

    /// <summary>
    /// Gets or sets the filter expression applied as an additional WHERE clause on the query.
    /// </summary>
    public Expression<Func<TEntity, bool>> Where { get; set; }

    /// <summary>
    /// Retrieves the entity with the specified key, applying optional includes and where conditions.
    /// </summary>
    /// <typeparam name="TKey">The type of the entity's primary key.</typeparam>
    /// <param name="id">The identifier of the entity to retrieve.</param>
    /// <param name="asNoTracking">Whether to disable EF change tracking.</param>
    /// <param name="ignoreQueryFilters">Whether to ignore global EF query filters.</param>
    /// <param name="asSplitQuery">Whether to use split queries for includes.</param>
    /// <returns>An <see cref="IResult"/> representing the HTTP response.</returns>
    public virtual async Task<IResult> Get<TKey>(TKey id, bool asNoTracking = true, bool ignoreQueryFilters = false, bool asSplitQuery = false) =>
        await Handle<TEntity, TGetDto>(new GetByIdRequest<TEntity, TKey>
        {
            Id = id,
            Includes = Includes ?? [],
            Where = Where ?? (x => true),
            AsNoTracking = asNoTracking,
            IgnoreQueryFilters = ignoreQueryFilters,
            AsSplitQuery = asSplitQuery,
        });
}
