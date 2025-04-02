using System.Linq.Expressions;
using CleanCodeJN.GenericApis.Abstractions.Responses;
using MediatR;

namespace CleanCodeJN.GenericApis.Commands;

/// <summary>
/// Get by Id request
/// </summary>
/// <typeparam name="TEntity">TEntity type.</typeparam>
/// <typeparam name="TKey">Type of the Id column.</typeparam>
public class GetByIdRequest<TEntity, TKey> : GetBaseRequest, IRequest<BaseResponse<TEntity>>
      where TEntity : class
{
    /// <summary>
    /// Id of the entity.
    /// </summary>
    public required TKey Id { get; init; }

    /// <summary>
    /// Includes for the query.
    /// </summary>
    public List<Expression<Func<TEntity, object>>> Includes { get; set; } = [];

    /// <summary>
    /// Where condition for the query.
    /// </summary>
    public Expression<Func<TEntity, bool>> Where { get; set; } = x => true;
}
