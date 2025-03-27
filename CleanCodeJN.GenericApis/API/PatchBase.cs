using AutoMapper;
using CleanCodeJN.GenericApis.Abstractions.Contracts;
using CleanCodeJN.GenericApis.Commands;
using MediatR;

namespace CleanCodeJN.GenericApis.API;

public abstract class PatchBase<TEntity, TGetDto, TKey>(IMediator commandBus, IMapper mapper) : ApiBase(commandBus, mapper)
    where TEntity : class
    where TGetDto : class, IDto
{
    public virtual async Task<IResult> Patch(TKey id, HttpContext httpContext) =>
        await Handle<TEntity, TGetDto>(new PatchRequest<TEntity, TKey> { Id = id, HttpContext = httpContext });
}
