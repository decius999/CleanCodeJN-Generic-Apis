using System.Linq.Expressions;
using AutoMapper;
using CleanCodeJN.GenericApis.Abstractions.Contracts;
using CleanCodeJN.GenericApis.Abstractions.Extensions;
using CleanCodeJN.GenericApis.Commands;
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

    public Expression<Func<TEntity, TEntity>> Select { get; set; }

    public virtual async Task<IResult> Get<TKey>(bool asNoTracking = true, bool ignoreQueryFilters = false, bool asSplitQuery = false) =>
        await Handle<TEntity, List<TGetDto>>(new GetRequest<TEntity, TKey>
        {
            Includes = Includes ?? [],
            Where = Where ?? (x => true),
            Select = Select,
            AsNoTracking = asNoTracking,
            IgnoreQueryFilters = ignoreQueryFilters,
            AsSplitQuery = asSplitQuery,
        });

    public virtual async Task<IResult> Get<TKey>(int page, int pageSize, string direction, string sortBy, bool asNoTracking = true, bool ignoreQueryFilters = false, bool asSplitQuery = false)
       => await Get<TKey>(page, pageSize, direction, sortBy, null, asNoTracking, ignoreQueryFilters, asSplitQuery);

    public virtual async Task<IResult> Get<TKey>(int page, int pageSize, string direction, string sortBy, string filter, bool asNoTracking = true, bool ignoreQueryFilters = false, bool asSplitQuery = false)
      => await HandlePagination<TEntity, TGetDto>(new GetRequest<TEntity, TKey>
      {
          Skip = page,
          Take = pageSize,
          SortOrder = direction.GetSortOrder(),
          SortField = sortBy,
          Filter = filter.ToFilter(),
          Includes = Includes ?? [],
          Where = Where ?? (x => true),
          Select = Select,
          AsNoTracking = asNoTracking,
          IgnoreQueryFilters = ignoreQueryFilters,
          AsSplitQuery = asSplitQuery,
      });
}
