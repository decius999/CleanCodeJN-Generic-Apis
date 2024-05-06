using AutoMapper;
using CleanCodeJN.GenericApis.Commands;
using CleanCodeJN.GenericApis.Contracts;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CleanCodeJN.GenericApis.API;

public abstract class GetBase<TEntity, TGetDto> : ApiBase
    where TEntity : class
    where TGetDto : class, IDto
{
    protected GetBase(IMediator commandBus, IMapper mapper) : base(commandBus, mapper)
    {
    }

    [HttpGet]
    public virtual async Task<IResult> Get() =>
        await Handle<TEntity, List<TGetDto>>(new GetRequest<TEntity>());
}
