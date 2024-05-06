using AutoMapper;
using CleanCodeJN.GenericApis.Contracts;
using MediatR;

namespace CleanCodeJN.GenericApis.API;

public class Delete<TEntity, TGetDto> : DeleteBase<TEntity, TGetDto>
    where TEntity : class
    where TGetDto : class, IDto
{
    public Delete(IMediator commandBus, IMapper mapper) : base(commandBus, mapper)
    {
    }
}
