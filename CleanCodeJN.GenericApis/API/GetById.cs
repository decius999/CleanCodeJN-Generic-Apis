using AutoMapper;
using CleanCodeJN.GenericApis.Contracts;
using MediatR;

namespace CleanCodeJN.GenericApis.API;

public class GetById<TEntity, TGetDto> : GetByIdBase<TEntity, TGetDto>
    where TEntity : class
    where TGetDto : class, IDto
{
    public GetById(IMediator commandBus, IMapper mapper) : base(commandBus, mapper)
    {
    }
}
