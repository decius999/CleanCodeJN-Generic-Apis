using System.Linq.Expressions;
using AutoMapper;
using CleanCodeJN.GenericApis.Commands;
using CleanCodeJN.GenericApis.Contracts;
using MediatR;

namespace CleanCodeJN.GenericApis.API;

public abstract class GetBase<TEntity, TGetDto> : ApiBase
    where TEntity : class
    where TGetDto : class, IDto
{
    protected GetBase(IMediator commandBus, IMapper mapper) : base(commandBus, mapper)
    {
    }

    public List<Expression<Func<TEntity, object>>> Includes { get; set; }

    public Expression<Func<TEntity, bool>> Where { get; set; }

    public virtual async Task<IResult> Get() =>
        await Handle<TEntity, List<TGetDto>>(new GetRequest<TEntity>
        {
            Includes = Includes ?? [],
            Where = Where ?? (x => true),
        });
}
