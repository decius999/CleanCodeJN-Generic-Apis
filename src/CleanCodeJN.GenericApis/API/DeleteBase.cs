using AutoMapper;
using CleanCodeJN.GenericApis.Abstractions.Contracts;
using CleanCodeJN.GenericApis.Commands;
using MediatR;

namespace CleanCodeJN.GenericApis.API;

public abstract class DeleteBase<TEntity, TGetDto>(IMediator commandBus, IMapper mapper) : ApiBase(commandBus, mapper)
    where TEntity : class
    where TGetDto : class, IDto
{
    public virtual async Task<IResult> Delete<TKey>(TKey id) =>
        await Handle<TEntity, TGetDto>(new DeleteRequest<TEntity, TKey> { Id = id });
}
