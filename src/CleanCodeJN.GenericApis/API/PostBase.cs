using CleanCodeJN.GenericApis.Abstractions.Contracts;
using CleanCodeJN.GenericApis.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CleanCodeJN.GenericApis.API;

/// <summary>
/// Abstract base class providing a POST endpoint implementation for creating a new entity.
/// </summary>
/// <typeparam name="TEntity">The entity type to create.</typeparam>
/// <typeparam name="TPostDto">The DTO type accepted in the request body.</typeparam>
/// <typeparam name="TGetDto">The DTO type returned in the response.</typeparam>
public abstract class PostBase<TEntity, TPostDto, TGetDto>(IMediator commandBus, ICleanCodeMapper mapper) : ApiBase(commandBus, mapper)
    where TEntity : class
    where TGetDto : class, IDto
    where TPostDto : class, IDto
{
    /// <summary>
    /// Creates a new entity from the provided DTO and returns the created entity mapped to the response DTO.
    /// </summary>
    /// <param name="dto">The DTO object representing the entity to create.</param>
    /// <returns>An <see cref="IResult"/> representing the HTTP response.</returns>
    public virtual async Task<IResult> Post([FromBody] TPostDto dto) =>
        await Handle<TEntity, TGetDto>(new PostRequest<TEntity, TPostDto> { Dto = dto });
}
