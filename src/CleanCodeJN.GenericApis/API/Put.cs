using AutoMapper;
using CleanCodeJN.GenericApis.Abstractions.Contracts;
using MediatR;

namespace CleanCodeJN.GenericApis.API;

/// <summary>
/// Default concrete implementation of <see cref="PutBase{TEntity,TPutDto,TGetDto}"/> used for dependency injection registration.
/// </summary>
/// <typeparam name="TEntity">The entity type to update.</typeparam>
/// <typeparam name="TPutDto">The DTO type used in the request body.</typeparam>
/// <typeparam name="TGetDto">The DTO type returned in the response.</typeparam>
public class Put<TEntity, TPutDto, TGetDto>(IMediator commandBus, IMapper mapper) : PutBase<TEntity, TPutDto, TGetDto>(commandBus, mapper)
    where TEntity : class
    where TPutDto : class, IDto
    where TGetDto : class, IDto
{
}
