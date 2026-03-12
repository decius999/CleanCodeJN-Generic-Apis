using System.Linq.Expressions;
using CleanCodeJN.GenericApis.Abstractions.Responses;
using MediatR;

namespace CleanCodeJN.GenericApis.Commands;

/// <summary>
/// Get Request
/// </summary>
/// <typeparam name="TEntity">TEntity type.</typeparam>
/// <typeparam name="TKey">Type of the Id column.</typeparam>
public class GetRequest<TEntity, TKey> : BasePaginationRequest, IRequest<BaseListResponse<TEntity>>
{
    /// <summary>
    /// Includes
    /// </summary>
    public List<Expression<Func<TEntity, object>>> Includes { get; set; } = [];

    /// <summary>
    /// Where clause
    /// </summary>
    public Expression<Func<TEntity, bool>> Where { get; set; } = x => true;

    /// <summary>
    /// Select clause
    /// </summary>
    public Expression<Func<TEntity, TEntity>> Select { get; set; }

    /// <summary>
    /// No tracking for entities.
    /// </summary>
    public bool AsNoTracking { get; init; }

    /// <summary>
    /// No global query filters.
    /// </summary>
    public bool IgnoreQueryFilters { get; init; }

    /// <summary>
    /// Use no joins on queries. Instead separate queries.
    /// </summary>
    public bool AsSplitQuery { get; init; }
}
