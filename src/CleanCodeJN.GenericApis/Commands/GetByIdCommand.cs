using CleanCodeJN.GenericApis.Abstractions.Responses;
using CleanCodeJN.Repository.EntityFramework.Contracts;
using MediatR;

namespace CleanCodeJN.GenericApis.Commands;

/// <summary>
/// Handles retrieval of a single entity by its primary key identifier.
/// </summary>
/// <typeparam name="TEntity">The entity type to retrieve.</typeparam>
/// <typeparam name="TKey">The type of the entity's primary key.</typeparam>
public class GetByIdCommand<TEntity, TKey>(IRepository<TEntity, TKey> repository) : IRequestHandler<GetByIdRequest<TEntity, TKey>, BaseResponse<TEntity>>
    where TEntity : class, IEntity<TKey>
{
    /// <summary>
    /// Queries the repository for an entity matching the specified ID, applying optional includes and where conditions.
    /// </summary>
    /// <param name="request">The get-by-id request containing the ID, includes, and filter expressions.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A <see cref="BaseResponse{TEntity}"/> with the entity if found, or a not-found failure result.</returns>
    public async Task<BaseResponse<TEntity>> Handle(GetByIdRequest<TEntity, TKey> request, CancellationToken cancellationToken)
    {
        var entity = repository
             .Query(
                asNoTracking: request.AsNoTracking,
                ignoreQueryFilters: request.IgnoreQueryFilters,
                asSplitQuery: request.AsSplitQuery,
                includes: request.Includes?.ToArray() ?? [])
            .Where(request.Where)
            .FirstOrDefault(x => x.Id.Equals(request.Id));

        return await BaseResponse<TEntity>.Create(
            entity is not null ? ResultEnum.SUCCESS : ResultEnum.FAILURE_NOT_FOUND,
            entity,
            message: entity is not null ? null : $"Id '{request.Id}' not found!");
    }
}
