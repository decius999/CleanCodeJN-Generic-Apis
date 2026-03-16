using CleanCodeJN.GenericApis.Abstractions.Contracts;
using CleanCodeJN.GenericApis.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CleanCodeJN.GenericApis.API;

/// <summary>
/// Abstract base class providing a PUT endpoint implementation for updating an existing entity.
/// </summary>
/// <typeparam name="TEntity">The entity type to update.</typeparam>
/// <typeparam name="TPutDto">The DTO type accepted in the request body.</typeparam>
/// <typeparam name="TGetDto">The DTO type returned in the response.</typeparam>
public abstract class PutBase<TEntity, TPutDto, TGetDto>(IMediator commandBus, ICleanCodeMapper mapper) : ApiBase(commandBus, mapper)
    where TEntity : class
    where TGetDto : class, IDto
    where TPutDto : class, IDto
{
    /// <summary>
    /// Updates an existing entity from the provided DTO and returns the updated entity mapped to the response DTO.
    /// </summary>
    /// <param name="dto">The DTO object representing the updated entity data.</param>
    /// <returns>An <see cref="IResult"/> representing the HTTP response.</returns>
    public virtual async Task<IResult> Put([FromBody] TPutDto dto) =>
        await Handle<TEntity, TGetDto>(new PutRequest<TEntity, TPutDto> { Dto = dto });
}
