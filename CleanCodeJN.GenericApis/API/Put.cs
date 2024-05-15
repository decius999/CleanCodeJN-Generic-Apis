using AutoMapper;
using CleanCodeJN.GenericApis.Abstractions.Contracts;
using MediatR;

namespace CleanCodeJN.GenericApis.API;

public class Put<TEntity, TPutDto, TGetDto> : PutBase<TEntity, TPutDto, TGetDto>
    where TEntity : class
    where TPutDto : class, IDto
    where TGetDto : class, IDto
{
    public Put(IMediator commandBus, IMapper mapper) : base(commandBus, mapper)
    {
    }
}
