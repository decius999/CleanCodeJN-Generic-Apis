using AutoMapper;
using CleanCodeJN.GenericApis.Abstractions.Contracts;
using CleanCodeJN.GenericApis.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CleanCodeJN.GenericApis.API;

public abstract class PostBase<TEntity, TPostDto, TGetDto>(IMediator commandBus, IMapper mapper) : ApiBase(commandBus, mapper)
    where TEntity : class
    where TGetDto : class, IDto
    where TPostDto : class, IDto
{
    public virtual async Task<IResult> Post([FromBody] TPostDto dto) =>
        await Handle<TEntity, TGetDto>(new PostRequest<TEntity, TPostDto> { Dto = dto });
}
