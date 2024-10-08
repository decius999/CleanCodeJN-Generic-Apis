using System.Linq.Expressions;
using CleanCodeJN.GenericApis.Abstractions.Responses;
using MediatR;

namespace CleanCodeJN.GenericApis.Commands;

public class GetByIdsRequest<TEntity, TKey> : GetBaseRequest, IRequest<BaseListResponse<TEntity>>
      where TEntity : class
{
    public List<TKey> Ids { get; init; }

    public List<Expression<Func<TEntity, object>>> Includes { get; set; } = [];
}
