using AutoMapper;
using CleanCodeJN.GenericApis.Commands;
using CleanCodeJN.GenericApis.Contracts;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CleanCodeJN.GenericApis.API;

public abstract class DeleteBase<TEntity, TGetDto> : ApiBase
    where TEntity : class
    where TGetDto : class, IDto
{
    protected DeleteBase(IMediator commandBus, IMapper mapper) : base(commandBus, mapper)
    {
    }

    [HttpDelete]
    public virtual async Task<IResult> Delete(int id) =>
        await Handle<TEntity, TGetDto>(new DeleteRequest<TEntity> { Id = id });
}
