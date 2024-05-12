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

    public virtual async Task<IResult> Get<TKey>() =>
        await Handle<TEntity, List<TGetDto>>(new GetRequest<TEntity, TKey>
        {
            Includes = Includes ?? [],
            Where = Where ?? (x => true),
        });

    public virtual async Task<IResult> Get<TKey>(int page, int pageSize, string direction, string sortBy)
       => await HandlePagination<TEntity, TGetDto>(new GetRequest<TEntity, TKey>
       {
           Skip = page,
           Take = pageSize,
           SortOrder = direction?.ToLowerInvariant() == "descending" ? "-1" : "1",
           SortField = sortBy,
           Includes = Includes ?? [],
           Where = Where ?? (x => true),
       });
}
