using CleanCodeJN.GenericApis.Abstractions.Responses;
using CleanCodeJN.GenericApis.Extensions;
using CleanCodeJN.Repository.EntityFramework.Contracts;
using MediatR;

namespace CleanCodeJN.GenericApis.Commands;

/// <summary>
/// Handles retrieval of a list of entities with optional filtering, sorting, and pagination.
/// </summary>
/// <typeparam name="TEntity">The entity type to retrieve.</typeparam>
/// <typeparam name="TKey">The type of the entity's primary key.</typeparam>
public class GetCommand<TEntity, TKey>(IRepository<TEntity, TKey> repository) : IRequestHandler<GetRequest<TEntity, TKey>, BaseListResponse<TEntity>>
    where TEntity : class, IEntity<TKey>
{
    /// <summary>
    /// Queries the repository for entities applying includes, where clause, select projection, pagination, and column filters.
    /// </summary>
    /// <param name="request">The get request containing query parameters such as includes, filters, and pagination settings.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A <see cref="BaseListResponse{TEntity}"/> containing the matching entities and total count.</returns>
    public async Task<BaseListResponse<TEntity>> Handle(GetRequest<TEntity, TKey> request, CancellationToken cancellationToken)
    {
        var query = repository
            .Query(
                asNoTracking: request.AsNoTracking,
                ignoreQueryFilters: request.IgnoreQueryFilters,
                asSplitQuery: request.AsSplitQuery,
                includes: request.Includes?.ToArray() ?? [])
            .Where(request.Where);

        if (request.Select is not null)
        {
            query = query.Select(request.Select);
        }

        var count = query.WhereColumnsContainFilter<TEntity, TKey>(request.Filter).Count();

        var entities = query.PagedResultList<TEntity, TKey>(
                     request.Skip,
                     request.Take,
                     request.SortField,
                     request.SortOrder,
                     request.Filter);

        return await BaseListResponse<TEntity>.Create(entities is not null, entities.ToList(), count: count);
    }
}
