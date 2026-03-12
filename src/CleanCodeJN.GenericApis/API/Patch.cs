using AutoMapper;
using CleanCodeJN.GenericApis.Abstractions.Contracts;
using MediatR;

namespace CleanCodeJN.GenericApis.API;

/// <summary>
/// Default concrete implementation of <see cref="PatchBase{TEntity,TGetDto,TKey}"/> used for dependency injection registration.
/// </summary>
/// <typeparam name="TEntity">The entity type to patch.</typeparam>
/// <typeparam name="TGetDto">The DTO type returned in the response.</typeparam>
/// <typeparam name="TKey">The type of the entity's primary key.</typeparam>
public class Patch<TEntity, TGetDto, TKey>(IMediator commandBus, IMapper mapper) : PatchBase<TEntity, TGetDto, TKey>(commandBus, mapper)
    where TEntity : class
    where TGetDto : class, IDto
{
}
