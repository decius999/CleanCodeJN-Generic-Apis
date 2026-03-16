using CleanCodeJN.GenericApis.Abstractions.Contracts;
using MediatR;

namespace CleanCodeJN.GenericApis.API;

/// <summary>
/// Default concrete implementation of <see cref="PostBase{TEntity,TPostDto,TGetDto}"/> used for dependency injection registration.
/// </summary>
/// <typeparam name="TEntity">The entity type to create.</typeparam>
/// <typeparam name="TPostDto">The DTO type used in the request body.</typeparam>
/// <typeparam name="TGetDto">The DTO type returned in the response.</typeparam>
public class Post<TEntity, TPostDto, TGetDto>(IMediator commandBus, ICleanCodeMapper mapper) : PostBase<TEntity, TPostDto, TGetDto>(commandBus, mapper)
    where TEntity : class
    where TPostDto : class, IDto
    where TGetDto : class, IDto
{
}
