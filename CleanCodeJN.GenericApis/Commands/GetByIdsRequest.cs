using System.Linq.Expressions;
using CleanCodeJN.GenericApis.Abstractions.Responses;
using MediatR;

namespace CleanCodeJN.GenericApis.Commands;

/// <summary>
/// Get by Ids Request
/// </summary>
/// <typeparam name="TEntity">TEntity type.</typeparam>
/// <typeparam name="TKey">Type of the Id column.</typeparam>
public class GetByIdsRequest<TEntity, TKey> : GetBaseRequest, IRequest<BaseListResponse<TEntity>>
      where TEntity : class
{
    /// <summary>
    /// The Ids to get.
    /// </summary>
    public List<TKey> Ids { get; init; }

    /// <summary>
    /// The includes to get.
    /// </summary>
    public List<Expression<Func<TEntity, object>>> Includes { get; set; } = [];
}
