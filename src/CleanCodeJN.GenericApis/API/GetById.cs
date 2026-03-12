using AutoMapper;
using CleanCodeJN.GenericApis.Abstractions.Contracts;
using MediatR;

namespace CleanCodeJN.GenericApis.API;

/// <summary>
/// Default concrete implementation of <see cref="GetByIdBase{TEntity,TGetDto}"/> used for dependency injection registration.
/// </summary>
/// <typeparam name="TEntity">The entity type to retrieve.</typeparam>
/// <typeparam name="TGetDto">The DTO type returned in the response.</typeparam>
public class GetById<TEntity, TGetDto>(IMediator commandBus, IMapper mapper) : GetByIdBase<TEntity, TGetDto>(commandBus, mapper)
    where TEntity : class
    where TGetDto : class, IDto
{
}
