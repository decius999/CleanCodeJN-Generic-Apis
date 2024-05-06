using AutoMapper;
using CleanCodeJN.GenericApis.Contracts;
using MediatR;

namespace CleanCodeJN.GenericApis.API;

public class Get<TEntity, TGetDto> : GetBase<TEntity, TGetDto>
    where TEntity : class
    where TGetDto : class, IDto
{
    public Get(IMediator commandBus, IMapper mapper) : base(commandBus, mapper)
    {
    }
}
