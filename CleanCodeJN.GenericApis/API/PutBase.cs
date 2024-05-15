using AutoMapper;
using CleanCodeJN.GenericApis.Abstractions.Contracts;
using CleanCodeJN.GenericApis.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CleanCodeJN.GenericApis.API;

public abstract class PutBase<TEntity, TPutDto, TGetDto> : ApiBase
    where TEntity : class
    where TGetDto : class, IDto
    where TPutDto : class, IDto
{
    protected PutBase(IMediator commandBus, IMapper mapper) : base(commandBus, mapper)
    {
    }

    public virtual async Task<IResult> Put([FromBody] TPutDto dto) =>
        await Handle<TEntity, TGetDto>(new PutRequest<TEntity, TPutDto> { Dto = dto });
}
