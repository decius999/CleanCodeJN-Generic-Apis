using AutoMapper;
using CleanCodeJN.GenericApis.Abstractions.Contracts;
using CleanCodeJN.GenericApis.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CleanCodeJN.GenericApis.API;

public abstract class PutBase<TEntity, TPutDto, TGetDto>(IMediator commandBus, IMapper mapper) : ApiBase(commandBus, mapper)
    where TEntity : class
    where TGetDto : class, IDto
    where TPutDto : class, IDto
{
    public virtual async Task<IResult> Put([FromBody] TPutDto dto) =>
        await Handle<TEntity, TGetDto>(new PutRequest<TEntity, TPutDto> { Dto = dto });
}
