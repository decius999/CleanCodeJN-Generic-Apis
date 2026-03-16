using System.Linq.Expressions;
using CleanCodeJN.GenericApis.Abstractions.Contracts;
using CleanCodeJN.GenericApis.Abstractions.Extensions;
using CleanCodeJN.GenericApis.Commands;
using MediatR;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace CleanCodeJN.GenericApis.API;

/// <summary>
/// Base API controller providing full CRUD operations for the specified entity type using MediatR.
/// </summary>
/// <typeparam name="TEntity">The entity type managed by this controller.</typeparam>
/// <typeparam name="TGetDto">The DTO type returned in GET responses.</typeparam>
/// <typeparam name="TPostDto">The DTO type accepted in POST request bodies.</typeparam>
/// <typeparam name="TPutDto">The DTO type accepted in PUT request bodies.</typeparam>
/// <typeparam name="TKey">The type of the entity's primary key.</typeparam>
[ApiController]
public class ApiCrudControllerBase<TEntity, TGetDto, TPostDto, TPutDto, TKey>(
    IMediator commandBus,
    ICleanCodeMapper mapper) : ApiBase(commandBus, mapper)
    where TEntity : class
    where TGetDto : class, IDto
    where TPostDto : class, IDto
    where TPutDto : class, IDto
{
    /// <summary>
    /// Gets or sets the navigation property includes applied to GET list queries.
    /// </summary>
    public virtual List<Expression<Func<TEntity, object>>> GetIncludes { get; set; } = [];

    /// <summary>
    /// Gets or sets the navigation property includes applied to GET-by-ID queries.
    /// </summary>
    public virtual List<Expression<Func<TEntity, object>>> GetByIdIncludes { get; set; } = [];

    /// <summary>
    /// Gets or sets the projection expression applied as a SELECT clause on GET list queries.
    /// </summary>
    public virtual Expression<Func<TEntity, TEntity>> GetSelect { get; set; }

    /// <summary>
    /// Gets or sets the filter expression applied as a WHERE clause on GET list queries.
    /// </summary>
    public virtual Expression<Func<TEntity, bool>> GetWhere { get; set; } = x => true;

    /// <summary>
    /// Gets or sets the filter expression applied as a WHERE clause on GET-by-ID queries.
    /// </summary>
    public virtual Expression<Func<TEntity, bool>> GetByIdWhere { get; set; } = x => true;

    /// <summary>
    /// Gets or sets a value indicating whether EF change tracking is disabled for queries.
    /// </summary>
    public virtual bool AsNoTracking { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether split queries are used for queries with includes.
    /// </summary>
    public virtual bool AsSplitQuery { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether global EF query filters are ignored.
    /// </summary>
    public virtual bool IgnoreQueryFilters { get; set; } = false;

    /// <summary>
    /// Retrieves a filtered and paged list of entities.
    /// </summary>
    /// <param name="page">The page index (0-based).</param>
    /// <param name="pageSize">The number of records per page.</param>
    /// <param name="direction">The sort direction.</param>
    /// <param name="sortBy">The property name to sort by.</param>
    /// <param name="filter">The filter string applied server-side to column values.</param>
    /// <returns>An <see cref="IResult"/> representing the HTTP response.</returns>
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

    /// <summary>
    /// Retrieves a paged list of entities sorted by the specified field and direction.
    /// </summary>
    /// <param name="page">The page index (0-based).</param>
    /// <param name="pageSize">The number of records per page.</param>
    /// <param name="direction">The sort direction.</param>
    /// <param name="sortBy">The property name to sort by.</param>
    /// <returns>An <see cref="IResult"/> representing the HTTP response.</returns>
    [HttpGet("paged")]
    public virtual async Task<IResult> Get(int page, int pageSize, string direction, string sortBy)
        => await Get(page, pageSize, direction, sortBy, null);

    /// <summary>
    /// Retrieves all entities without pagination.
    /// </summary>
    /// <returns>An <see cref="IResult"/> representing the HTTP response.</returns>
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

    /// <summary>
    /// Retrieves a single entity by its primary key.
    /// </summary>
    /// <param name="id">The identifier of the entity to retrieve.</param>
    /// <returns>An <see cref="IResult"/> representing the HTTP response.</returns>
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

    /// <summary>
    /// Creates a new entity from the provided DTO.
    /// </summary>
    /// <param name="dto">The DTO object representing the entity to create.</param>
    /// <returns>An <see cref="IResult"/> representing the HTTP response.</returns>
    [HttpPost]
    public virtual async Task<IResult> Post([FromBody] TPostDto dto) =>
        await Handle<TEntity, TGetDto>(new PostRequest<TEntity, TPostDto> { Dto = dto });

    /// <summary>
    /// Updates an existing entity from the provided DTO.
    /// </summary>
    /// <param name="dto">The DTO object representing the updated entity data.</param>
    /// <returns>An <see cref="IResult"/> representing the HTTP response.</returns>
    [HttpPut]
    public virtual async Task<IResult> Put([FromBody] TPutDto dto) =>
        await Handle<TEntity, TGetDto>(new PutRequest<TEntity, TPutDto> { Dto = dto });

    /// <summary>
    /// Applies a JSON Patch document to partially update the entity with the specified key.
    /// </summary>
    /// <param name="id">The identifier of the entity to patch.</param>
    /// <param name="patchDocument">The JSON Patch document describing the changes to apply.</param>
    /// <returns>An <see cref="IResult"/> representing the HTTP response.</returns>
    [HttpPatch("{id}")]
    public virtual async Task<IResult> Patch(TKey id, [FromBody] JsonPatchDocument<TEntity> patchDocument) =>
       await Handle<TEntity, TGetDto>(new PatchRequest<TEntity, TKey> { Id = id, PatchDocument = patchDocument });

    /// <summary>
    /// Deletes the entity with the specified key.
    /// </summary>
    /// <param name="id">The identifier of the entity to delete.</param>
    /// <returns>An <see cref="IResult"/> representing the HTTP response.</returns>
    [HttpDelete("{id}")]
    public virtual async Task<IResult> Delete(TKey id) =>
        await Handle<TEntity, TGetDto>(new DeleteRequest<TEntity, TKey> { Id = id });
}
