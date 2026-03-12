using AutoMapper;
using CleanCodeJN.GenericApis.Abstractions.Contracts;
using MediatR;

namespace CleanCodeJN.GenericApis.API;

public class Post<TEntity, TPostDto, TGetDto>(IMediator commandBus, IMapper mapper) : PostBase<TEntity, TPostDto, TGetDto>(commandBus, mapper)
    where TEntity : class
    where TPostDto : class, IDto
    where TGetDto : class, IDto
{
}
