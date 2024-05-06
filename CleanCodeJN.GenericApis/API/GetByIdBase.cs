using AutoMapper;
using CleanCodeJN.GenericApis.Commands;
using CleanCodeJN.GenericApis.Contracts;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CleanCodeJN.GenericApis.API;

public abstract class GetByIdBase<TEntity, TGetDto> : ApiBase
    where TEntity : class
    where TGetDto : class, IDto
{
    protected GetByIdBase(IMediator commandBus, IMapper mapper) : base(commandBus, mapper)
    {
    }

    [HttpGet]
    public virtual async Task<IResult> Get(int id) =>
        await Handle<TEntity, TGetDto>(new GetByIdRequest<TEntity> { Id = id });
}
