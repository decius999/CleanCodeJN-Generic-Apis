using AutoMapper;
using CleanCodeJN.GenericApis.Commands;
using CleanCodeJN.GenericApis.Contracts;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CleanCodeJN.GenericApis.API;

public abstract class PostBase<TEntity, TPostDto, TGetDto> : ApiBase
    where TEntity : class
    where TGetDto : class, IDto
    where TPostDto : class, IDto
{
    protected PostBase(IMediator commandBus, IMapper mapper) : base(commandBus, mapper)
    {
    }

    [HttpPost]
    public virtual async Task<IResult> Post([FromBody] TPostDto dto) =>
        await Handle<TEntity, TGetDto>(new PostRequest<TEntity, TPostDto> { Dto = dto });
}
