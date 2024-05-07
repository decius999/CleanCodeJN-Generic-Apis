using System.Linq.Expressions;
using AutoMapper;
using CleanCodeJN.GenericApis.Commands;
using CleanCodeJN.GenericApis.Contracts;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CleanCodeJN.GenericApis.API;

[ApiController]
public class ApiCrudControllerBase<TEntity, TGetDto, TPostDto, TPutDto>(
    IMediator commandBus,
    IMapper mapper) : ApiBase(commandBus, mapper)
    where TEntity : class
    where TGetDto : class, IDto
    where TPostDto : class, IDto
    where TPutDto : class, IDto
{
    public virtual List<Expression<Func<TEntity, object>>> GetIncludes { get; set; } = [];

    public virtual List<Expression<Func<TEntity, object>>> GetByIdIncludes { get; set; } = [];

    public virtual Expression<Func<TEntity, bool>> GetWhere { get; set; } = x => true;

    public virtual Expression<Func<TEntity, bool>> GetByIdWhere { get; set; } = x => true;

    [HttpGet()]
    public virtual async Task<IResult> Get() =>
        await Handle<TEntity, List<TGetDto>>(new GetRequest<TEntity> { Includes = GetIncludes, Where = GetWhere });

    [HttpGet("{id:int}")]
    public virtual async Task<IResult> Get(int id) =>
        await Handle<TEntity, TGetDto>(new GetByIdRequest<TEntity> { Id = id, Includes = GetByIdIncludes, Where = GetByIdWhere });

    [HttpPost]
    public virtual async Task<IResult> Post([FromBody] TPostDto dto) =>
        await Handle<TEntity, TGetDto>(new PostRequest<TEntity, TPostDto> { Dto = dto });

    [HttpPut]
    public virtual async Task<IResult> Put([FromBody] TPutDto dto) =>
        await Handle<TEntity, TGetDto>(new PutRequest<TEntity, TPutDto> { Dto = dto });

    [HttpDelete("{id:int}")]
    public virtual async Task<IResult> Delete(int id) =>
        await Handle<TEntity, TGetDto>(new DeleteRequest<TEntity> { Id = id });
}
