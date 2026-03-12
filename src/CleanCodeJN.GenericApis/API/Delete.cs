using AutoMapper;
using CleanCodeJN.GenericApis.Abstractions.Contracts;
using MediatR;

namespace CleanCodeJN.GenericApis.API;

/// <summary>
/// Default concrete implementation of <see cref="DeleteBase{TEntity,TGetDto}"/> used for dependency injection registration.
/// </summary>
/// <typeparam name="TEntity">The entity type to delete.</typeparam>
/// <typeparam name="TGetDto">The DTO type returned after deletion.</typeparam>
public class Delete<TEntity, TGetDto>(IMediator commandBus, IMapper mapper) : DeleteBase<TEntity, TGetDto>(commandBus, mapper)
    where TEntity : class
    where TGetDto : class, IDto
{
}
