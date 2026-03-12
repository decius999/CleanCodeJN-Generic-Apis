using AutoMapper;
using CleanCodeJN.GenericApis.Abstractions.Contracts;
using MediatR;

namespace CleanCodeJN.GenericApis.API;

public class Put<TEntity, TPutDto, TGetDto>(IMediator commandBus, IMapper mapper) : PutBase<TEntity, TPutDto, TGetDto>(commandBus, mapper)
    where TEntity : class
    where TPutDto : class, IDto
    where TGetDto : class, IDto
{
}
