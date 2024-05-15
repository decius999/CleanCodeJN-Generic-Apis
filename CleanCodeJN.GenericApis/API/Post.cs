using AutoMapper;
using CleanCodeJN.GenericApis.Abstractions.Contracts;
using MediatR;

namespace CleanCodeJN.GenericApis.API;

public class Post<TEntity, TPostDto, TGetDto> : PostBase<TEntity, TPostDto, TGetDto>
    where TEntity : class
    where TPostDto : class, IDto
    where TGetDto : class, IDto
{
    public Post(IMediator commandBus, IMapper mapper) : base(commandBus, mapper)
    {
    }
}
