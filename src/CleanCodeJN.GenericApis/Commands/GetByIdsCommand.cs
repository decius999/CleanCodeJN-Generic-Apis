using CleanCodeJN.GenericApis.Abstractions.Responses;
using CleanCodeJN.Repository.EntityFramework.Contracts;
using MediatR;

namespace CleanCodeJN.GenericApis.Commands;

/// <summary>
/// Handles retrieval of multiple entities by a list of primary key identifiers.
/// </summary>
/// <typeparam name="TEntity">The entity type to retrieve.</typeparam>
/// <typeparam name="TKey">The type of the entity's primary key.</typeparam>
public class GetByIdsCommand<TEntity, TKey>(IRepository<TEntity, TKey> repository) : IRequestHandler<GetByIdsRequest<TEntity, TKey>, BaseListResponse<TEntity>>
    where TEntity : class, IEntity<TKey>
{
    /// <summary>
    /// Queries the repository for all entities whose IDs are contained in the request's ID list.
    /// </summary>
    /// <param name="request">The get-by-ids request containing the list of IDs and optional includes.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A <see cref="BaseListResponse{TEntity}"/> containing the matching entities.</returns>
    public async Task<BaseListResponse<TEntity>> Handle(GetByIdsRequest<TEntity, TKey> request, CancellationToken cancellationToken)
    {
        var entities = repository
             .Query(
                asNoTracking: request.AsNoTracking,
                ignoreQueryFilters: request.IgnoreQueryFilters,
                asSplitQuery: request.AsSplitQuery,
                includes: request.Includes?.ToArray() ?? [])
            .Where(x => request.Ids.Contains(x.Id))
            .ToList();

        return await BaseListResponse<TEntity>.Create(entities?.Any() == true, entities);
    }
}
