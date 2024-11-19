using System.Linq.Expressions;
using AutoMapper;
using CleanCodeJN.GenericApis.Abstractions.Contracts;
using CleanCodeJN.GenericApis.Abstractions.Extensions;
using CleanCodeJN.GenericApis.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CleanCodeJN.GenericApis.API;

[ApiController]
public class ApiCrudControllerBase<TEntity, TGetDto, TPostDto, TPutDto, TKey>(
    IMediator commandBus,
    IMapper mapper) : ApiBase(commandBus, mapper)
    where TEntity : class
    where TGetDto : class, IDto
    where TPostDto : class, IDto
    where TPutDto : class, IDto
{
    public virtual List<Expression<Func<TEntity, object>>> GetIncludes { get; set; } = [];

    public virtual List<Expression<Func<TEntity, object>>> GetByIdIncludes { get; set; } = [];

    public virtual Expression<Func<TEntity, TEntity>> GetSelect { get; set; }

    public virtual Expression<Func<TEntity, bool>> GetWhere { get; set; } = x => true;

    public virtual Expression<Func<TEntity, bool>> GetByIdWhere { get; set; } = x => true;

    public virtual bool AsNoTracking { get; set; } = true;

    public virtual bool AsSplitQuery { get; set; } = true;

    public virtual bool IgnoreQueryFilters { get; set; } = false;

    [HttpGet("filtered")]
    public virtual async Task<IResult> Get(int page, int pageSize, string direction, string sortBy, string filter)
       => await HandlePagination<TEntity, TGetDto>(new GetRequest<TEntity, TKey>
       {
           Skip = page,
           Take = pageSize,
           SortOrder = direction.GetSortOrder(),
           SortField = sortBy,
           Filter = filter.ToFilter(),
           Includes = GetIncludes,
           Where = GetWhere,
           Select = GetSelect,
           AsNoTracking = AsNoTracking,
           IgnoreQueryFilters = IgnoreQueryFilters,
           AsSplitQuery = AsSplitQuery,
       });

    [HttpGet("paged")]
    public virtual async Task<IResult> Get(int page, int pageSize, string direction, string sortBy)
        => await Get(page, pageSize, direction, sortBy, null);

    [HttpGet()]
    public virtual async Task<IResult> Get() =>
        await Handle<TEntity, List<TGetDto>>(new GetRequest<TEntity, TKey>
        {
            Includes = GetIncludes,
            Where = GetWhere,
            Select = GetSelect,
            AsNoTracking = AsNoTracking,
            IgnoreQueryFilters = IgnoreQueryFilters,
            AsSplitQuery = AsSplitQuery,
        });

    [HttpGet("{id}")]
    public virtual async Task<IResult> Get(TKey id) =>
        await Handle<TEntity, TGetDto>(new GetByIdRequest<TEntity, TKey>
        {
            Id = id,
            Includes = GetByIdIncludes,
            Where = GetByIdWhere,
            AsNoTracking = AsNoTracking,
            IgnoreQueryFilters = IgnoreQueryFilters,
            AsSplitQuery = AsSplitQuery,
        });

    [HttpPost]
    public virtual async Task<IResult> Post([FromBody] TPostDto dto) =>
        await Handle<TEntity, TGetDto>(new PostRequest<TEntity, TPostDto> { Dto = dto });

    [HttpPut]
    public virtual async Task<IResult> Put([FromBody] TPutDto dto) =>
        await Handle<TEntity, TGetDto>(new PutRequest<TEntity, TPutDto> { Dto = dto });

    [HttpDelete("{id}")]
    public virtual async Task<IResult> Delete(TKey id) =>
        await Handle<TEntity, TGetDto>(new DeleteRequest<TEntity, TKey> { Id = id });
}
