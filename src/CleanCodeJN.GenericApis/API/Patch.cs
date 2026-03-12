using AutoMapper;
using CleanCodeJN.GenericApis.Abstractions.Contracts;
using MediatR;

namespace CleanCodeJN.GenericApis.API;

public class Patch<TEntity, TGetDto, TKey>(IMediator commandBus, IMapper mapper) : PatchBase<TEntity, TGetDto, TKey>(commandBus, mapper)
    where TEntity : class
    where TGetDto : class, IDto
{
}
