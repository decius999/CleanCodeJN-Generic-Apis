using CleanCodeJN.GenericApis.Abstractions.Contracts;
using CleanCodeJN.GenericApis.Commands;
using MediatR;

namespace CleanCodeJN.GenericApis.API;

/// <summary>
/// Abstract base class providing a DELETE endpoint implementation for the specified entity type.
/// </summary>
/// <typeparam name="TEntity">The entity type to delete.</typeparam>
/// <typeparam name="TGetDto">The DTO type returned in the response after deletion.</typeparam>
public abstract class DeleteBase<TEntity, TGetDto>(IMediator commandBus, ICleanCodeMapper mapper) : ApiBase(commandBus, mapper)
    where TEntity : class
    where TGetDto : class, IDto
{
    /// <summary>
    /// Deletes the entity with the specified key and returns the deleted entity mapped to the response DTO.
    /// </summary>
    /// <typeparam name="TKey">The type of the entity's primary key.</typeparam>
    /// <param name="id">The identifier of the entity to delete.</param>
    /// <returns>An <see cref="IResult"/> representing the HTTP response.</returns>
    public virtual async Task<IResult> Delete<TKey>(TKey id) =>
        await Handle<TEntity, TGetDto>(new DeleteRequest<TEntity, TKey> { Id = id });
}
