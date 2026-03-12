using CleanCodeJN.GenericApis.Abstractions.Responses;
using CleanCodeJN.Repository.EntityFramework.Contracts;
using MediatR;

namespace CleanCodeJN.GenericApis.Commands;

/// <summary>
/// Handles the deletion of an entity by its primary key identifier.
/// </summary>
/// <typeparam name="TEntity">The entity type to delete.</typeparam>
/// <typeparam name="TKey">The type of the entity's primary key.</typeparam>
public class DeleteCommand<TEntity, TKey>(IRepository<TEntity, TKey> repository) : IRequestHandler<DeleteRequest<TEntity, TKey>, BaseResponse<TEntity>>
    where TEntity : class, IEntity<TKey>
{
    /// <summary>
    /// Deletes the entity with the specified ID from the repository.
    /// </summary>
    /// <param name="request">The delete request containing the entity ID.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A <see cref="BaseResponse{TEntity}"/> with the deleted entity, or a not-found failure result.</returns>
    public async Task<BaseResponse<TEntity>> Handle(DeleteRequest<TEntity, TKey> request, CancellationToken cancellationToken)
    {
        var entity = await repository.Delete(request.Id, cancellationToken);

        return await BaseResponse<TEntity>.Create(
           entity is not null ? ResultEnum.SUCCESS : ResultEnum.FAILURE_NOT_FOUND,
           entity,
           message: entity is not null ? null : $"Id '{request.Id}' not found!");
    }
}
