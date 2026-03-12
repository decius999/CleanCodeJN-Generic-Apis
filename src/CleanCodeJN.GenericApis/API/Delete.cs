using AutoMapper;
using CleanCodeJN.GenericApis.Abstractions.Contracts;
using MediatR;

namespace CleanCodeJN.GenericApis.API;

public class Delete<TEntity, TGetDto>(IMediator commandBus, IMapper mapper) : DeleteBase<TEntity, TGetDto>(commandBus, mapper)
    where TEntity : class
    where TGetDto : class, IDto
{
}
