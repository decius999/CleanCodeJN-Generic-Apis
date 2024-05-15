using AutoMapper;
using CleanCodeJN.GenericApis.Abstractions.Contracts;
using CleanCodeJN.GenericApis.Commands;
using MediatR;

namespace CleanCodeJN.GenericApis.API;

public abstract class DeleteBase<TEntity, TGetDto> : ApiBase
    where TEntity : class
    where TGetDto : class, IDto
{
    protected DeleteBase(IMediator commandBus, IMapper mapper) : base(commandBus, mapper)
    {
    }

    public virtual async Task<IResult> Delete<TKey>(TKey id) =>
        await Handle<TEntity, TGetDto>(new DeleteRequest<TEntity, TKey> { Id = id });
}
