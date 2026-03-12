using AutoMapper;
using CleanCodeJN.GenericApis.Abstractions.Contracts;
using CleanCodeJN.GenericApis.Commands;
using MediatR;

namespace CleanCodeJN.GenericApis.API;

/// <summary>
/// Abstract base class providing a PATCH endpoint implementation for partially updating an existing entity.
/// </summary>
/// <typeparam name="TEntity">The entity type to patch.</typeparam>
/// <typeparam name="TGetDto">The DTO type returned in the response.</typeparam>
/// <typeparam name="TKey">The type of the entity's primary key.</typeparam>
public abstract class PatchBase<TEntity, TGetDto, TKey>(IMediator commandBus, IMapper mapper) : ApiBase(commandBus, mapper)
    where TEntity : class
    where TGetDto : class, IDto
{
    /// <summary>
    /// Applies a JSON Patch document from the HTTP request body to the entity with the specified key.
    /// </summary>
    /// <param name="id">The identifier of the entity to patch.</param>
    /// <param name="httpContext">The current HTTP context used to read the patch document from the request body.</param>
    /// <returns>An <see cref="IResult"/> representing the HTTP response.</returns>
    public virtual async Task<IResult> Patch(TKey id, HttpContext httpContext) =>
        await Handle<TEntity, TGetDto>(new PatchRequest<TEntity, TKey> { Id = id, HttpContext = httpContext });
}
