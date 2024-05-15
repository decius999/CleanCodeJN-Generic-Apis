using System.Linq.Expressions;
using AutoMapper;
using CleanCodeJN.GenericApis.Abstractions.Contracts;
using CleanCodeJN.GenericApis.Commands;
using MediatR;

namespace CleanCodeJN.GenericApis.API;

public abstract class GetByIdBase<TEntity, TGetDto> : ApiBase
    where TEntity : class
    where TGetDto : class, IDto
{
    protected GetByIdBase(IMediator commandBus, IMapper mapper) : base(commandBus, mapper)
    {
    }

    public List<Expression<Func<TEntity, object>>> Includes { get; set; }

    public Expression<Func<TEntity, bool>> Where { get; set; }

    public virtual async Task<IResult> Get<TKey>(TKey id) =>
        await Handle<TEntity, TGetDto>(new GetByIdRequest<TEntity, TKey>
        {
            Id = id,
            Includes = Includes ?? [],
            Where = Where ?? (x => true),
        });
}
